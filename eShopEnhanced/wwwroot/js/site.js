// ===== Add to / remove from cart without a page reload =====
// "js-cart-form" forms (_AddToCartButton, _QuickAddControl) are posted with fetch; the
// X-Requested-With header makes the server answer with JSON instead of redirecting, so the
// page keeps its scroll position and the search dropdown stays open.
document.addEventListener('submit', async function (event) {
    const form = event.target.closest('form.js-cart-form');
    if (!form) {
        return;
    }
    event.preventDefault();

    const button = form.querySelector('button');
    button.disabled = true;   // no double clicks while the request is on its way

    try {
        const response = await fetch(form.action, {
            method: 'POST',
            body: new FormData(form),   // includes the anti-forgery token
            headers: { 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' }
        });
        if (!response.ok) {
            throw new Error('HTTP ' + response.status);
        }
        const cart = await response.json();

        updateCartControls(cart.productId, cart.amount);
        updateCartBadge(cart.cartCount, cart.cartTotal, cart.cartTotalText);
        if (cart.message) {
            showToast(cart.message);
        }
    } catch (error) {
        form.submit();   // fall back to a normal post
    } finally {
        button.disabled = false;
    }
});

// Every control for that product on the page (a card AND a search suggestion can both show it)
function updateCartControls(productId, amount) {
    document.querySelectorAll('.cart-control[data-product-id="' + productId + '"]').forEach(function (control) {
        control.querySelector('.cart-control-add').hidden = amount > 0;
        control.querySelector('.cart-control-stepper').hidden = amount === 0;
        control.querySelector('.cart-control-amount').textContent = amount;
    });
}

// The navbar: "Cart (3) · 53,10 €". The total text arrives already formatted by the server.
function updateCartBadge(count, total, totalText) {
    const badge = document.querySelector('.nav-cart-badge');
    if (badge) {
        badge.textContent = count;
        badge.hidden = count === 0;
    }
    const totalElement = document.querySelector('.nav-cart-total');
    if (totalElement) {
        totalElement.textContent = totalText;
        totalElement.hidden = !(total > 0);
    }
}

// Client-side version of the _Layout toast. The message goes in via textContent so it can't inject HTML.
function showToast(message) {
    let container = document.querySelector('.cart-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container position-fixed bottom-0 end-0 p-3 cart-toast-container';
        document.body.appendChild(container);
    }
    container.innerHTML =
        '<div class="toast cart-toast" role="status" aria-live="polite" aria-atomic="true" data-bs-delay="3000">' +
        '  <div class="d-flex align-items-center">' +
        '    <div class="toast-body"><span class="cart-toast-check" aria-hidden="true">&#10003;</span> <span class="cart-toast-text"></span>' +
        '      <a href="/ShoppingCart" class="cart-toast-link" data-bs-toggle="offcanvas" data-bs-target="#miniCart" aria-controls="miniCart">View cart &rarr;</a></div>' +
        '    <button type="button" class="btn-close me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>' +
        '  </div>' +
        '</div>';
    container.querySelector('.cart-toast-text').textContent = message;
    bootstrap.Toast.getOrCreateInstance(container.querySelector('.toast')).show();
}

// Navbar search: from 2 letters, show matching products in a dropdown under the box.
const searchInput = document.querySelector('.nav-search-input');

if (searchInput) {
    const searchResults = document.getElementById('navSearchResults');
    let typingTimer = null;
    let currentRequest = null;   // aborted when a newer search starts

    function hideSearchResults() {
        searchResults.hidden = true;
    }

    searchInput.addEventListener('input', function () {
        const query = searchInput.value.trim();
        clearTimeout(typingTimer);

        if (query.length < 2) {
            hideSearchResults();
            return;
        }

        // Debounce: search once typing pauses for 250 ms
        typingTimer = setTimeout(async function () {
            if (currentRequest) {
                currentRequest.abort();
            }
            currentRequest = new AbortController();

            try {
                // returnUrl: the non-JS quick-add fallback brings the visitor back to this page
                const url = searchInput.dataset.suggestUrl + '?q=' + encodeURIComponent(query) +
                    '&returnUrl=' + encodeURIComponent(location.pathname + location.search);
                const response = await fetch(url, { signal: currentRequest.signal });
                if (!response.ok) {
                    throw new Error('HTTP ' + response.status);
                }
                searchResults.innerHTML = await response.text();
                searchResults.hidden = false;
            } catch (error) {
                if (error.name !== 'AbortError') {
                    hideSearchResults();   // on a real error just hide; Enter still opens the search page
                }
            }
        }, 250);
    });

    // Escape closes the dropdown
    searchInput.addEventListener('keydown', function (event) {
        if (event.key === 'Escape') {
            hideSearchResults();
        }
    });

    // Clicking anywhere outside the search box closes it; focusing the box again reopens it
    document.addEventListener('click', function (event) {
        if (!event.target.closest('.nav-search')) {
            hideSearchResults();
        }
    });
    searchInput.addEventListener('focus', function () {
        if (searchInput.value.trim().length >= 2 && searchResults.innerHTML.trim() !== '') {
            searchResults.hidden = false;
        }
    });
}

// Show server-rendered toasts (e.g. the TempData "added to cart" message)
document.querySelectorAll('.toast').forEach(function (toastElement) {
    bootstrap.Toast.getOrCreateInstance(toastElement).show();
});

// Slide-out cart: load /ShoppingCart/MiniCart each time it opens, rather than rendering the
// cart on every page where nobody may look at it.
const miniCart = document.getElementById('miniCart');

if (miniCart) {
    const miniCartBody = document.getElementById('miniCartBody');

    miniCart.addEventListener('show.bs.offcanvas', async function () {
        // If it was opened from the toast's "View cart" link, close the toast
        document.querySelectorAll('.toast.show').forEach(function (toastElement) {
            bootstrap.Toast.getOrCreateInstance(toastElement).hide();
        });

        miniCartBody.innerHTML = '<p class="mini-cart-loading">Loading your cart&hellip;</p>';

        try {
            const response = await fetch(miniCartBody.dataset.url);
            if (!response.ok) {
                throw new Error('HTTP ' + response.status);
            }
            miniCartBody.innerHTML = await response.text();
        } catch (error) {
            miniCartBody.innerHTML =
                '<p>Sorry, your cart could not be loaded.</p>' +
                '<a class="btn btn-primary" href="/ShoppingCart">Open the cart page</a>';
        }
    });
}
