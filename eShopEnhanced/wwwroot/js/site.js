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
