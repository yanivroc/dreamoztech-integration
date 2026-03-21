// Global variables for reCAPTCHA and summary amounts
let recaptchaToken = null;
let recaptchaTokenContact = null;
let recaptchaTokenPayment = null;
let subtotal = 0;
let DELIVERY_COST = 0;
let total = 0;

// Square Web Payments SDK objects
let payments;
let card; // To hold the Square Card object

// Global constant for storage key
const CART_STORAGE_KEY = 'shoppingCartSession'; // Or 'shoppingCart' if you prefer localStorage

// Global references to DOM elements - these will be initialized inside DOMContentLoaded
let cartContentDiv;
let subtotalAmountSpan;
let totalAmountSpan;
let cartItemCountSpan;
let deliveryCheckbox;
let deliveryAmountSpan;
let scrollToTopBtn;
let popupOverlayElement; // Global reference for the popup element
let toastElement; // Reference to the toast element with ID "toast"
let payButton; // Reference to the payment submit button

// Flag to prevent multiple payment submissions
let isProcessingPayment = false;


//console.log("Cart page script loaded."); // Added for debugging

// This function is called by the Google Maps API script once it's loaded.
function initAutocomplete() {
    const addressInput = document.getElementById('customer-address');
    if (addressInput) { // Ensure the element exists
        const autocomplete = new google.maps.places.Autocomplete(addressInput, {
            types: ['geocode'],
            componentRestrictions: { country: 'au' }
        });
        autocomplete.addListener('place_changed', () => {
            const place = autocomplete.getPlace();
            if (!place.geometry) {
                //console.error("No details available for input: '" + place.name + "'");
                addressInput.value = '';
                return;
            }
            addressInput.value = place.formatted_address || '';
            //console.log('Selected Address Details:', place);
        });
    } else {
        console.warn("Google Maps Autocomplete: 'customer-address' input not found.");
    }
}

// This function is called by reCAPTCHA when the user successfully completes the challenge.
function recaptchaCompleted(token) {
    recaptchaToken = token;
    //console.log('reCAPTCHA Subscription token received:', recaptchaToken);
}

function recaptchaCompletedContact(token) {
    recaptchaTokenContact = token;
    //console.log('reCAPTCHA Contact token received:', recaptchaTokenContact);
}

function recaptchaCompletedPayment(token) {
    recaptchaTokenPayment = token;
    //console.log('reCAPTCHA Payment token received:', recaptchaTokenPayment);
}

// Function to retrieve cart items from sessionStorage
function getCartItems() {
    try {
        const cartData = localStorage.getItem(CART_STORAGE_KEY); // Using localStorage as per your previous code
        return cartData ? JSON.parse(cartData) : [];
    } catch (e) {
        console.error("Error parsing cart data from localStorage:", e);
        return [];
    }
}

// Helper function to show the toast message
function showToast(message, isError = false) {
    if (!toastElement) {
        console.warn("Toast element with ID 'toast' not found. Please ensure it's in your HTML.");
        return;
    }

    toastElement.textContent = message;
    // Remove both classes first to ensure correct state
    toastElement.classList.remove('error', 'success');

    if (isError) {
        toastElement.classList.add('error');
    } else {
        toastElement.classList.add('success');
    }

    toastElement.classList.add("show");
    // Hide the toast after 3 seconds
    setTimeout(function () {
        toastElement.classList.remove("show");
    }, 3000); // 3000 milliseconds = 3 seconds
}

// Function to calculate and update summary values
//function updateSummary() {
//    const cart = getCartItems();
//    subtotal = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);

//    if (subtotalAmountSpan) subtotalAmountSpan.textContent = `$${subtotal.toFixed(2)}`;

//    // Calculate DELIVERY_COST first based on checkbox state and cart quantity
//    if (deliveryCheckbox && deliveryCheckbox.checked) {
//        let totalQuantity = cart.reduce((sum, item) => sum + item.quantity, 0);
//        if (totalQuantity <= 6) {
//            DELIVERY_COST = 9.99;
//        } else if (totalQuantity > 6 && totalQuantity <= 12) {
//            DELIVERY_COST = 12.99;
//        } else {
//            DELIVERY_COST = 15.00;
//        }
//        if (deliveryAmountSpan) {
//            deliveryAmountSpan.textContent = `$${DELIVERY_COST.toFixed(2)}`;
//            deliveryAmountSpan.style.display = 'inline';
//        }
//    } else {
//        DELIVERY_COST = 0;
//        if (deliveryAmountSpan) {
//            deliveryAmountSpan.textContent = `$0.00`;
//            deliveryAmountSpan.style.display = 'none';
//        }
//    }

//    total = DELIVERY_COST + subtotal;
//    if (totalAmountSpan) totalAmountSpan.textContent = `$${total.toFixed(2)}`;
//}

function updateSummary() {
    const cart = getCartItems();
    // 1. Calculate the subtotal (cost of items only)
    const subtotal = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);

    if (subtotalAmountSpan) subtotalAmountSpan.textContent = `$${subtotal.toFixed(2)}`;

    // 2. Determine delivery cost based on your new rules
    if (deliveryCheckbox && deliveryCheckbox.checked) {
        // If subtotal is 100 or more, it's free ($0), otherwise it's $10
        DELIVERY_COST = subtotal >= 100 ? 0 : 10.00;

        if (deliveryAmountSpan) {
            // Show "Free" or the dollar amount
            deliveryAmountSpan.textContent = DELIVERY_COST === 0 ? "Free" : `$${DELIVERY_COST.toFixed(2)}`;
            deliveryAmountSpan.style.display = 'inline';
        }
    } else {
        // No delivery selected (e.g., Pickup)
        DELIVERY_COST = 0;
        if (deliveryAmountSpan) {
            deliveryAmountSpan.style.display = 'none';
        }
    }

    // 3. Calculate final total
    const total = subtotal + DELIVERY_COST;
    if (totalAmountSpan) totalAmountSpan.textContent = `$${total.toFixed(2)}`;
}

// Initialize Square Web Payments SDK
async function initializeSquarePayments() {
    // Get the application ID from the hidden input field
    const applicationId = document.getElementById("application-id").value;

    // Get the location ID from the hidden input field
    const locationId = document.getElementById("location-id").value;

    if (!applicationId || !locationId) {
        console.error("Square Application ID or Location ID is not set. Please configure them.");
        showToast("Payment system not configured. Please contact support.", true);
        return;
    }

    try {
        payments = Square.payments(applicationId, locationId);

        card = await payments.card({}); // Initialize the Card payment method

        const cardContainer = document.getElementById('card-container');
        if (cardContainer) {
            await card.attach('#card-container');
            //console.log("Square Web Payments SDK initialized successfully.");
        } else {
            //console.warn("Square Card container with ID 'card-container' not found. Card input will not render.");
            showToast('Payment card input area not found. Please check HTML.', true);
        }
    } catch (e) {
        console.error('Initializing Square Payments failed:', e);
        showToast('Failed to load payment system. Please try again later.', true);
    }
}

// This function is called when the "Pay" button is clicked or form is submitted.
async function processPayment(event) {
    event.preventDefault();

    if (isProcessingPayment) {
        console.warn("Payment already in process. Please wait.");
        return false; // Prevent multiple submissions
    }

    if (!recaptchaTokenPayment) {
        showToast('Please complete the reCAPTCHA verification.');
        return false;
    }

    isProcessingPayment = true; // Set flag to true
    if (payButton) {
        payButton.disabled = true; // Disable the button
        payButton.textContent = 'Processing...'; // Optional: provide feedback
    }

    try {
        const cart = getCartItems();
        // Corrected: Map item.itemPath to id
        const cartItems = cart.map(item => ({
            id: item.itemPath, // Changed from item.id to item.itemPath
            name: item.name,
            price: item.price,
            quantity: item.quantity
        }));

        if (cartItems.length === 0) {
            showToast('Your cart is empty. Please add items before paying.', true);
            return false;
        }

        const customerName = document.getElementById('customer-name').value.trim();
        const customerPhone = document.getElementById('customer-phone').value.trim();
        const customerEmail = document.getElementById('customer-email').value.trim();
        const customerAddress = document.getElementById('customer-address').value.trim();
        const customerNotes = document.getElementById('customer-notes').value.trim();

        const errors = [];
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(customerEmail)) errors.push('Invalid email format.');

        if (errors.length > 0) {
            showToast('Please correct the following errors:\n' + errors.join('\n'), true);
            return false;
        }

        let paymentToken = null;
        if (!card) {
            showToast('Payment system not ready. Please try again.', true);
            return false;
        }

        const result = await card.tokenize(); // Tokenization happens here
        if (result.status === 'OK') {
            paymentToken = result.token;
            //console.log('Square Payment Token:', paymentToken);
        } else {
            let errorMessage = `Card tokenization failed: ${result.status}`;
            if (result.errors && result.errors.length > 0) {
                errorMessage += ` Errors: ${JSON.stringify(result.errors.map(e => e.detail || e.code))}`;
            }
            showToast(errorMessage, true);
            console.error('Square Tokenization Error:', result.errors);
            return false;
        }

        const paymentData = {
            cartItems: cartItems,
            summary: {
                subtotal: subtotal,
                deliveryCost: DELIVERY_COST,
                total: total
            },
            customerDetails: {
                name: customerName,
                phone: customerPhone,
                email: customerEmail,
                address: customerAddress,
                notes: customerNotes
            },
            paymentToken: paymentToken,
            recaptchaResponse: recaptchaTokenPayment
        };

        //console.log('Sending Payment Data to Backend:', paymentData);

        const response = await fetch('/Newsletter/pay', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(paymentData)
        });

        if (!response.ok) {
            let errorDetails = 'Unknown error.';
            try {
                const errorJson = await response.json();
                errorDetails = errorJson.message || JSON.stringify(errorJson);
            } catch (e) {
                // If response is not JSON, get raw text
                errorDetails = await response.text();
            }
            throw new Error(`HTTP error! status: ${response.status}. Details: ${errorDetails}`);
        }

        const resultData = await response.json();
        //console.log('Payment successful:', resultData);

        showToast('Payment successful! Thank you for your order.');
        emptyCartAndRefresh();
        clearCustomerForm();
        return true; // Indicate success
    } catch (error) {
        console.error('Payment failed:', error);
        showToast('Payment failed: ' + error.message + '. Please try again or contact support.', true);
        if (typeof grecaptcha !== 'undefined' && grecaptcha.reset) {
            grecaptcha.reset();
        }
        recaptchaToken = null;
        return false; // Indicate failure
    } finally {
        isProcessingPayment = false; // Reset flag
        if (payButton) {
            payButton.disabled = false; // Re-enable the button
            payButton.textContent = 'Pay Now'; // Reset button text
        }
    }
}

// Function to empty the cart, clear localStorage, and update the UI
function emptyCartAndRefresh() {
    // Clear the cart from localStorage
    localStorage.removeItem(CART_STORAGE_KEY);

    // Reset the global summary variables
    subtotal = 0;
    DELIVERY_COST = 0;
    total = 0;

    // Rerender the cart display (which will show the "Your cart is empty" message)
    renderCart();

    // Update the cart count in the header/navigation
    updateCartCount();

    // Hide cart
    hideCartSection();
}

// Function to clear the customer details form
function clearCustomerForm() {
    const formFields = [
        'customer-name',
        'customer-phone',
        'customer-email',
        'customer-address',
        'customer-notes'
    ];
    formFields.forEach(fieldId => {
        const element = document.getElementById(fieldId);
        if (element) {
            element.value = '';
        }
    });

    // Clear the Square card input field by calling the card.clear() method
    if (card) {
        card.clear();
    }
}

// Function to hide the cart section
function hideCartSection() {
    const cartSection = document.getElementById('Cart');
    if (cartSection) {
        cartSection.style.display = 'none';
    }
}

// Function to show the cart section
function showCartSection() {
    const cartSection = document.getElementById('Cart');
    if (cartSection) {
        cartSection.style.display = 'block';
    }
}

// Function to add item to cart
function addToCart(itemName, itemPrice, itemPath, itemImageUrl) {
    let cart = getCartItems();
    const existingItem = cart.find(item => item.itemPath === itemPath);

    if (existingItem) {
        if (existingItem.quantity < 6) {
            existingItem.quantity++;
            showToast(`${itemName} added to cart!`);
        } else {
            //console.log(`Maximum quantity (6) reached for ${itemName}.`);
            showToast(`Maximum quantity (6) reached for ${itemName}.`);
        }
    } else {
        cart.push({
            name: itemName,
            price: itemPrice,
            itemPath: itemPath,
            quantity: 1,
            imagePath: itemImageUrl
        });
        showToast(`${itemName} added to cart!`);
    }

    saveCartItems(cart);
    updateCartCount();
    renderCart();
}

// Function to save cart items to localStorage
function saveCartItems(cart) {
    try {
        localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(cart));
    } catch (e) {
        console.error("Error saving cart to localStorage:", e);
    }
}

// Function to update item quantity in cart
function updateQuantity(itemPath, newQuantity) {
    let cart = getCartItems();
    cart = cart.map(item => {
        if (item.itemPath === itemPath) {
            const quantity = Math.max(1, Math.min(6, newQuantity));
            return { ...item, quantity };
        }
        return item;
    }).filter(item => item.quantity > 0);

    saveCartItems(cart);
    updateCartCount();
    renderCart();
}

// Function to remove item from cart
function removeItem(itemPath) {
    let cart = getCartItems();
    cart = cart.filter(item => item.itemPath !== itemPath);
    saveCartItems(cart);
    updateCartCount();
    renderCart();
}

// Function to render cart items in the HTML
function renderCart() {
    const cart = getCartItems();
    subtotal = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);

    //if (deliveryCheckbox && deliveryCheckbox.checked) {
    //    let totalQuantity = cart.reduce((sum, item) => sum + item.quantity, 0);
    //    if (totalQuantity <= 6) {
    //        DELIVERY_COST = 9.99;
    //    } else if (totalQuantity > 6 && totalQuantity <= 12) {
    //        DELIVERY_COST = 12.99;
    //    } else {
    //        DELIVERY_COST = 15.00;
    //    }
    //    if (deliveryAmountSpan) {
    //        deliveryAmountSpan.textContent = `$${DELIVERY_COST.toFixed(2)}`;
    //        deliveryAmountSpan.style.display = 'inline';
    //    }
    //} else {
    //    DELIVERY_COST = 0;
    //    if (deliveryAmountSpan) {
    //        deliveryAmountSpan.textContent = `$0.00`;
    //        deliveryAmountSpan.style.display = 'none';
    //    }
    //}
    //total = DELIVERY_COST + subtotal;

    if (deliveryCheckbox && deliveryCheckbox.checked) {
        // New Logic: If subtotal is $100 or more, delivery is $0. Otherwise, it's $10.
        DELIVERY_COST = subtotal >= 100 ? 0 : 10.00;

        if (deliveryAmountSpan) {
            // Display "Free" if cost is 0, otherwise show the $10.00
            deliveryAmountSpan.textContent = DELIVERY_COST === 0 ? "Free" : `$${DELIVERY_COST.toFixed(2)}`;
            deliveryAmountSpan.style.display = 'inline';
        }
    } else {
        DELIVERY_COST = 0;
        if (deliveryAmountSpan) {
            deliveryAmountSpan.style.display = 'none';
        }
    }

    total = DELIVERY_COST + subtotal;

    let cartContentHtml = '';
    if (cart.length === 0) {
        cartContentHtml = '<h2 class="text-center text-gray-600 text-lg custom-h1">Your cart is empty.</h2>';
        hideCartSection();
    } else {
        showCartSection();
        cartContentHtml = `
                                <table class="cart-table">
                                    <thead>
                                        <tr>
                                            <th class="w-2/5">Product</th>
                                            <th class="w-1/5">Price</th>
                                            <th class="w-1/5">Quantity</th>
                                            <th class="w-1/5 text-right">Total</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        ${cart.map(item => `
                                            <tr>
                                                <td class="product-col" data-label="Product">
                                                    <img
                                                        src="${item.imagePath}"
                                                        alt="${item.name}"
                                                        class="w-20 h-20 object-cover rounded-md flex-shrink-0"
                                                        onerror="this.onerror=null; this.src='https://placehold.co/80x80/CCCCCC/000000?text=No+Image';"
                                                    />
                                                    <div>
                                                        <h3 class="text-lg font-semibold text-gray-900">${item.name}</h3>
                                                    </div>
                                                </td>
                                                <td data-label="Price">
                                                    <p class="text-gray-800 font-medium">$${item.price.toFixed(2)}</p>
                                                </td>
                                                <td data-label="Quantity">
                                                    <div class="flex items-center space-x-2">
                                                        <div class="quantity-controls">
                                                            <button
                                                                data-item-path="${item.itemPath}"
                                                                data-action="decrease"
                                                                ${item.quantity <= 1 ? 'disabled' : ''}
                                                                class="quantity-btn"
                                                            >
                                                                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-minus"><path d="M5 12h14"/></svg>
                                                            </button>
                                                            <span class="text-lg font-medium text-gray-800 text-center">
                                                                ${item.quantity}
                                                            </span>
                                                            <button
                                                                data-item-path="${item.itemPath}"
                                                                data-action="increase"
                                                                ${item.quantity >= 1 ? 'disabled' : ''}
                                                                class="quantity-btn"
                                                            >
                                                                <svg xmlns="http://www.w3.org/2000/svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-plus"><path d="M12 5v14"/><path d="M5 12h14"/></svg>
                                                            </button>
                                                            <button
                                                                data-item-path="${item.itemPath}"
                                                                class="remove-item-btn"
                                                                data-label="Remove"
                                                            >
                                                                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="lucide lucide-x-circle"><circle cx="12" cy="12" r="10"/><path d="m15 9-6 6"/><path d="m9 9 6 6"/></svg>
                                                            </button>
                                                        </div>
                                                    </div>
                                                </td>
                                                <td class="text-right" data-label="Total">
                                                    <p class="text-lg font-semibold text-indigo-600">$${(item.price * item.quantity).toFixed(2)}</p>
                                                </td>
                                            </tr>
                                        `).join('')}
                                    </tbody>
                                </table>
                    `;
    }

    if (cartContentDiv) {
        cartContentDiv.innerHTML = cartContentHtml;
    }

    if (subtotalAmountSpan) subtotalAmountSpan.textContent = `$${subtotal.toFixed(2)}`;
    if (totalAmountSpan) totalAmountSpan.textContent = `$${total.toFixed(2)}`;

    document.querySelectorAll('.quantity-btn').forEach(button => {
        button.addEventListener('click', (event) => {
            const itemPath = event.currentTarget.dataset.itemPath;
            const action = event.currentTarget.dataset.action;
            let currentQuantity = parseInt(event.currentTarget.closest('.quantity-controls').querySelector('span').textContent);
            updateQuantity(itemPath, currentQuantity + (action === 'increase' ? 1 : -1));
        });
    });

    document.querySelectorAll('.remove-item-btn').forEach(button => {
        button.addEventListener('click', (event) => {
            const itemPath = event.currentTarget.dataset.itemPath;
            removeItem(itemPath);
        });
    });
}

// Function to update the cart count display
function updateCartCount() {
    const cart = getCartItems();
    const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
    const cartCountElement = document.getElementById('cart-item-count');

    if (cartCountElement) {
        cartCountElement.textContent = totalItems;
        if (totalItems === 0) {
            cartCountElement.style.display = 'none';
        } else {
            cartCountElement.style.display = 'inline-block';
        }
    }
}

// --- Contact Form Logic ---
async function handleContactFormSubmit(event) {
    event.preventDefault();

    const form = event.target;
    const formData = new FormData(form);
    const name = formData.get('name');
    const email = formData.get('email');
    const message = formData.get('message');

    if (!recaptchaTokenContact) {
        showToast('Please complete the reCAPTCHA verification for the contact form.');
        return false;
    }

    const data = {
        name: name,
        email: email,
        message: message,
        recaptchaToken: recaptchaTokenContact
    };

    try {
        const response = await fetch(form.action, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            const result = await response.json();
            showToast(result.message || 'Your message has been sent!');
            form.reset();
        } else {
            const errorText = await response.text();
            console.error("Server responded with non-OK status for contact form. Response body:", errorText);
            try {
                const errorData = JSON.parse(errorText);
                showToast(errorData.message || 'Failed to send message. Please try again.');
            } catch (e) {
                showToast('Failed to send message. Server returned an unexpected response.');
            }
        }
    } catch (error) {
        console.error('Error during contact form submission (network or fetch issue):', error);
        showToast('An unexpected error occurred. Please check your network connection.');
    } finally {
        if (typeof grecaptcha !== 'undefined' && grecaptcha.reset) {
            grecaptcha.reset();
        }
        recaptchaTokenContact = null;
    }
}

// --- Subscription Form Logic ---
async function handleSubscription(event) {
    event.preventDefault();

    const form = event.target;
    const formData = new FormData(form);
    const name = formData.get('name');
    const email = formData.get('email');

    if (!recaptchaToken) {
        showToast('Please complete the reCAPTCHA verification.');
        return false;
    }

    const data = {
        name: name,
        email: email,
        recaptchaToken: recaptchaToken
    };
    try {
        const response = await fetch(form.action, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            },
            body: JSON.stringify(data)
        });

        if (response.ok) {
            const result = await response.json();
            showToast(result.message || 'Subscription successful!');
            form.reset();
        } else {
            const errorData = await response.json();
            showToast(errorData.message || 'Subscription failed. Please try again.');
        }
    } catch (error) {
        console.error('Error during subscription:', error);
        showToast('An unexpected error occurred. Please check your network connection.');
    } finally {
        if (typeof grecaptcha !== 'undefined' && grecaptcha.reset) {
            grecaptcha.reset();
        }
    }
}

// This function runs when the initial HTML document has been completely loaded and parsed.
document.addEventListener('DOMContentLoaded', function () {
    // Initialize DOM element references here
    const hamburgerMenu = document.querySelector('.hamburger-menu');
    const nav = document.getElementById('mainNav');
    const closeMenuBtn = document.querySelector('.close-menu-btn');
    const body = document.body;

    // Assign global variables here within DOMContentLoaded
    cartContentDiv = document.getElementById('cart-content-container');
    subtotalAmountSpan = document.getElementById('subtotal-amount');
    totalAmountSpan = document.getElementById('total-amount');
    cartItemCountSpan = document.getElementById('cart-item-count');
    deliveryCheckbox = document.getElementById('delivery-checkbox');
    deliveryAmountSpan = document.getElementById('delivery-amount');
    scrollToTopBtn = document.getElementById("scrollToTopBtn");
    popupOverlayElement = document.getElementById("popup");
    toastElement = document.getElementById("toast"); // Initialize toastElement here
    payButton = document.getElementById('pay-button'); // Initialize payButton here

    function toggleMenu() {
        if (nav) {
            nav.classList.toggle('active');
        }
        if (hamburgerMenu) {
            hamburgerMenu.classList.toggle('active');
        }
        body.classList.toggle('menu-open');
    }

    if (hamburgerMenu && nav) {
        hamburgerMenu.addEventListener('click', toggleMenu);
    }

    if (closeMenuBtn && nav) {
        closeMenuBtn.addEventListener('click', toggleMenu);
    }

    if (nav) {
        nav.querySelectorAll('a:not(.close-menu-btn)').forEach(link => {
            link.addEventListener('click', () => {
                if (nav.classList.contains('active')) {
                    toggleMenu();
                }
            });
        });
    }

    document.addEventListener('click', function (event) {
        if (nav && nav.classList.contains('active') &&
            !nav.contains(event.target) &&
            !hamburgerMenu.contains(event.target)) {
            toggleMenu();
        }
    });

    // --- Slider functionality ---
    const slidesContainer = document.querySelector('.slides-container');
    const slides = document.querySelectorAll('.slide');
    const prevButton = document.querySelector('.prev-button');
    const nextButton = document.querySelector('.next-button');
    const dotsContainer = document.querySelector('.dots-container');
    const dots = document.querySelectorAll('.dot');

    let currentIndex = 0;
    const totalSlides = slides.length;

    function updateSlider() {
        if (slidesContainer) {
            slidesContainer.style.transform = `translateX(-${currentIndex * 100}%)`;
        }
        updateDots();
    }

    function updateDots() {
        dots.forEach((dot, index) => {
            if (index === currentIndex) {
                dot.classList.add('active');
            } else {
                dot.classList.remove('active');
            }
        });
    }

    if (nextButton) {
        nextButton.addEventListener('click', () => {
            currentIndex = (currentIndex + 1) % totalSlides;
            updateSlider();
        });
    }

    if (prevButton) {
        prevButton.addEventListener('click', () => {
            currentIndex = (currentIndex - 1 + totalSlides) % totalSlides;
            updateSlider();
        });
    }

    if (dotsContainer) {
        dots.forEach(dot => {
            dot.addEventListener('click', (event) => {
                const slideIndex = parseInt(event.target.dataset.slideIndex);
                currentIndex = slideIndex;
                updateSlider();
            });
        });
    }

    if (totalSlides > 1) {
        setInterval(() => {
            currentIndex = (currentIndex + 1) % totalSlides;
            updateSlider();
        }, 5000);
    }

    if (slidesContainer) {
        updateSlider();
    }

    // --- Scroll to Top Button Logic ---
    if (scrollToTopBtn) {
        window.onscroll = function () {
            if (document.body.scrollTop > 30 || document.documentElement.scrollTop > 30) {
                scrollToTopBtn.style.display = "block";
            } else {
                scrollToTopBtn.style.display = "none";
            }
        };

        scrollToTopBtn.onclick = function () {
            document.body.scrollTop = 0;
            document.documentElement.scrollTop = 0;
        };
    } else {
        console.warn("Scroll to top button with ID 'scrollToTopBtn' not found.");
    }

    // --- MAIN POPUP LOGIC INITIALIZATION ---
    // Show popup on initial page load (once per session)
    showPopupIfNeverShown();

    // Initial calls when DOM is ready
    renderCart();
    updateCartCount();
    initializeSquarePayments();

    if (deliveryCheckbox) {
        deliveryCheckbox.addEventListener('change', renderCart);
    }

    // Attach processPayment to the form's submit event
    const checkoutForm = document.getElementById('checkout-form');
    if (checkoutForm) {
        checkoutForm.addEventListener('submit', processPayment);
    }
});

// --- Functions that need to be globally accessible (outside DOMContentLoaded) ---

/**
 * Attempts to show the popup if it hasn't been shown in the current session.
 * Sets a sessionStorage flag if it shows the popup.
 */
function showPopupIfNeverShown() {
    // popupOverlayElement is now initialized within DOMContentLoaded, so it will be available
    // when this function is called after DOMContentLoaded has completed.
    if (!popupOverlayElement) { // This check is still good as a safeguard
        console.warn("Popup element with ID 'popup' not found in showPopupIfNeverShown. This might indicate a timing issue or missing HTML element.");
        return;
    }

    const hasPopupBeenShown = sessionStorage.getItem('hasPopupBeenShown');

    if (!hasPopupBeenShown) {
        popupOverlayElement.style.display = "flex";
        sessionStorage.setItem('hasPopupBeenShown', 'true');
        //console.log("Popup shown for the first time this session.");
    } else {
        //console.log("Popup already shown this session, skipping.");
    }
}

/**
 * Closes the popup. This also ensures the sessionStorage flag is set
 * so it doesn't reappear via exit-intent if closed manually.
 */
function closePopup() {
    if (!popupOverlayElement) { // This check is still good as a safeguard
        console.warn("Popup element with ID 'popup' not found in closePopup. This might indicate a timing issue or missing HTML element.");
        return;
    }
    popupOverlayElement.style.display = "none";
    sessionStorage.setItem('hasPopupBeenShown', 'true');
    //console.log("Popup closed by user.");
}

// Close popup and navigate to #products
function orderNowAndNavigate() {
    closePopup();
    window.location.href = '#Products';
}
