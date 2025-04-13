document.addEventListener("DOMContentLoaded", function () {
    function updateTotal() {
        let total = 0;
        let itemCount = 0;

        document.querySelectorAll(".shopping-item-card").forEach(card => {
            const quantity = parseInt(card.querySelector(".quantity").textContent);
            const price = parseFloat(card.querySelector(".item-price").textContent.replace("$", "").trim());
            total += quantity * price;
            itemCount += quantity;
        });

        const shippingSelect = document.querySelector("select[name='fees']");
        const shippingCost = parseFloat(shippingSelect.value) || 0;

        const totalWithShipping = total + shippingCost;

        document.querySelector(".summary-items").textContent = `ITEMS ${itemCount}`;
        document.querySelector(".summary-total").textContent = `$ ${total.toFixed(2)}`;
        document.querySelector(".summary-total-with-fees").textContent = `$ ${totalWithShipping.toFixed(2)}`;
    }

    document.querySelector("select[name='fees']").addEventListener("change", updateTotal);

    function removeFromDb(productId) {
        $.post("/Cart/RemoveFromCart", { productId: productId })
            .done(() => console.log("Item removed"))
            .fail(err => console.error("Error removing item", err));
    }

    function updateQuantity(productId, newQuantity) {
        $.post("/Cart/UpdateQuantity", { productId: productId, newQuantity: newQuantity })
            .done(() => console.log("Quantity updated"))
            .fail(err => console.error("Error updating quantity", err));
    }

    document.querySelectorAll(".shopping-item-card").forEach(card => {
        const minusBtn = card.querySelector(".minus");
        const plusBtn = card.querySelector(".plus");
        const quantityText = card.querySelector(".quantity");
        const closeBtn = card.querySelector(".close");

        const productId = card.dataset.productId;

        if (minusBtn && plusBtn && quantityText) {
            minusBtn.addEventListener("click", function (e) {
                e.preventDefault();
                let quantity = parseInt(quantityText.textContent);
                if (quantity > 1) {
                    quantityText.textContent = quantity - 1;
                    updateQuantity(productId, quantityText.textContent);
                    updateTotal();
                }
            });

            plusBtn.addEventListener("click", function (e) {
                e.preventDefault();
                let quantity = parseInt(quantityText.textContent);
                quantityText.textContent = quantity + 1;
                updateQuantity(productId, quantityText.textContent);
                updateTotal();
            });
        }

        if (closeBtn) {
            closeBtn.addEventListener("click", function (e) {
                e.preventDefault();
                removeFromDb(productId);
                card.remove();
                updateTotal();
            });
        }
    });
    document.querySelector("#checkout-button").addEventListener("click", function () {
        const paymentMethod = document.querySelector("input[name='payment']:checked")?.value || "CreditCard";
        const shippingFees = document.querySelector("select[name='fees']").value || 0;

        $.post("/Checkout/ProcessCheckout", { payment: paymentMethod, fees: shippingFees })
            .done(response => console.log("Checkout successful:", response))
            .fail(error => console.error("Checkout failed:", error));
    });

    updateTotal();
});