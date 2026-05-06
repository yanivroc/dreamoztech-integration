using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DreamozTech.Models;
using Square;
using Square.Models;

namespace DreamozTech.Service
{
    public class SquareService : ISquareService
    {
        private readonly SquareClient _squareClient;

        public SquareService(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            var accessToken = configuration["Square:AccessToken"];
            var env = configuration["Square:Environment"] ?? "Sandbox";

            var environment = env.Equals("Production", StringComparison.OrdinalIgnoreCase)
                ? Square.Environment.Production
                : Square.Environment.Sandbox;

            _squareClient = new SquareClient.Builder()
                .Environment(environment)
                .AccessToken(accessToken)
                .Build();
        }

        public async Task<List<SquareProduct>> GetAllSquareProductsAsync()
        {
            var products = new List<SquareProduct>();

            var catalogApi = _squareClient.CatalogApi;

            string? cursor = null;

            while (true)
            {
                var response = await catalogApi.ListCatalogAsync(
                    cursor: cursor,
                    types: "ITEM,IMAGE"
                );

                if (response.Errors != null && response.Errors.Any())
                {
                    throw new Exception(
                        string.Join(", ", response.Errors.Select(e => e.Detail))
                    );
                }

                var objects = response.Objects ?? new List<CatalogObject>();

                // Build image lookup from any IMAGE objects returned in this page
                var imageLookup = objects
                    .Where(o => string.Equals(o.Type, "IMAGE", StringComparison.OrdinalIgnoreCase) && o.ImageData?.Url != null)
                    .ToDictionary(o => o.Id!, o => o.ImageData!.Url!);

                // Collect image ids referenced by items that are not yet in imageLookup
                var referencedImageIds = objects
                    .Where(o => string.Equals(o.Type, "ITEM", StringComparison.OrdinalIgnoreCase))
                    .SelectMany(o => o.ItemData?.ImageIds ?? Enumerable.Empty<string>())
                    .Where(id => !string.IsNullOrEmpty(id) && !imageLookup.ContainsKey(id))
                    .Distinct()
                    .ToList();

                // Batch-retrieve any missing image objects (preferred over repeated single retrieves)
                if (referencedImageIds.Any())
                {
                    try
                    {
                        var batchReq = new BatchRetrieveCatalogObjectsRequest(
                            referencedImageIds, // required IList<string> objectIds
                            null,               // includeRelatedObjects
                            null,               // catalogVersion
                            null                // includeDeletedObjects
                        );

                        var batchResp = await catalogApi.BatchRetrieveCatalogObjectsAsync(batchReq);

                        if (batchResp?.Objects != null)
                        {
                            foreach (var imgObj in batchResp.Objects.Where(o => string.Equals(o.Type, "IMAGE", StringComparison.OrdinalIgnoreCase) && o.ImageData?.Url != null))
                            {
                                imageLookup[imgObj.Id!] = imgObj.ImageData!.Url!;
                            }
                        }
                    }
                    catch
                    {
                        // swallow - missing images are non-critical
                    }
                }

                foreach (var obj in objects.Where(o => string.Equals(o.Type, "ITEM", StringComparison.OrdinalIgnoreCase)))
                {
                    var itemData = obj.ItemData;
                    // In SDK v22 'IsDeleted' is on CatalogObject (obj)
                    if (itemData == null || obj.IsDeleted == true)
                        continue;

                    var product = new SquareProduct
                    {
                        ItemId = obj.Id ?? string.Empty,
                        Name = itemData.Name ?? string.Empty,
                        Description = itemData.Description ?? string.Empty
                    };

                    // Image (optional)
                    if (itemData.ImageIds?.Any() == true)
                    {
                        var imageId = itemData.ImageIds.First();
                        if (!string.IsNullOrEmpty(imageId) && imageLookup.TryGetValue(imageId, out var imageUrl))
                        {
                            product.ImageUrl = imageUrl;
                        }
                    }

                    // Variations + pricing
                    foreach (var variation in itemData.Variations ?? Enumerable.Empty<CatalogObject>())
                    {
                        var variationData = variation.ItemVariationData;
                        if (variationData?.PriceMoney == null)
                            continue;

                        product.Variations.Add(new SquareProductVariation
                        {
                            VariationId = variation.Id ?? string.Empty,
                            Name = variationData.Name ?? string.Empty,
                            Price = (variationData.PriceMoney.Amount ?? 0) / 100m,
                            Currency = variationData.PriceMoney.Currency ?? string.Empty
                        });
                    }

                    products.Add(product);
                }

                if (string.IsNullOrEmpty(response.Cursor))
                    break;

                cursor = response.Cursor;
            }

            return products;
        }
    }
}
