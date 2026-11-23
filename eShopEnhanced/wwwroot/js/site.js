// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Keep the page still when adding to the cart.
// "Add to cart" submits a form, which reloads the page and would normally start at the top.
// So: just before the form is sent, remember where we were; after the reload, go straight back there.
// (sessionStorage = a small per-tab storage in the browser; try/catch because it can be blocked.)
document.addEventListener('submit', function (event) {
    if (event.target.matches('.add-to-cart-form')) {
        try {
            sessionStorage.setItem('restoreScroll', JSON.stringify({
                page: location.pathname + location.search,
                y: window.scrollY
            }));
        } catch (e) { /* storage unavailable: the page will simply start at the top */ }
    }
});

try {
    const saved = JSON.parse(sessionStorage.getItem('restoreScroll'));
    sessionStorage.removeItem('restoreScroll');
    // Only restore on the page the visitor was on (not after "+" on the cart page etc.)
    if (saved && saved.page === location.pathname + location.search) {
        // behavior 'instant': Bootstrap turns on smooth scrolling, which would visibly glide down from the top
        window.scrollTo({ top: saved.y, behavior: 'instant' });
    }
} catch (e) { /* nothing saved, or storage unavailable */ }

// Navbar search: from 2 letters, show matching products in a dropdown under the box.
const searchInput = document.querySelector('.nav-search-input');

if (searchInput) {
    const searchResults = document.getElementById('navSearchResults');
    let typingTimer = null;
    let currentRequest = null;   // lets us cancel a search that's still running when the visitor types on

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

        // Wait until the visitor pauses typing for 250 ms, instead of searching on every single key
        typingTimer = setTimeout(async function () {
            if (currentRequest) {
                currentRequest.abort();
            }
            currentRequest = new AbortController();

            try {
                const url = searchInput.dataset.suggestUrl + '?q=' + encodeURIComponent(query);
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

// Show any toast messages on the page (e.g. "Black Forest Cake was added to your cart.").
// Bootstrap toasts are hidden until JavaScript shows them; they hide themselves after data-bs-delay ms.
document.querySelectorAll('.toast').forEach(function (toastElement) {
    bootstrap.Toast.getOrCreateInstance(toastElement).show();
});

// Slide-out cart panel: every time it starts opening, fetch the current cart contents
// from /ShoppingCart/MiniCart and put them into the panel.
// Loading on open (instead of on every page) means pages don't run a cart query nobody looks at.
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
