var popup = document.querySelector(".addedcheck");
var addtolistbtn = document.querySelectorAll(" .large-btn");

addtolistbtn.forEach(button => {
    console.log("maro")
    button.addEventListener("click", function () {
        button.innerHTML = `<i class="uil uil-check"></i> Added`;
        popup.classList.replace("hidecheck", "showcheck");
        setTimeout(() => {
            button.innerHTML = "Add To Cart";
            popup.classList.replace("showcheck", "hidecheck");
        }, 2500);
    });
});

document.querySelectorAll(".choose-container div").forEach((choice) => {
    choice.addEventListener("click", function () {
        document.querySelectorAll(".choose-container div").forEach((el) => {
            el.querySelector("span").classList.replace("choice-choosen", "choice-notchoosen");
            el.querySelector("hr").classList.replace("choice-choosen-bar", "choice-notchoosen-bar");
        });
        var reviews = document.querySelector(".reviews");
        var description = document.querySelector(".description");
        if (this.textContent.trim().includes("Description")) {
            reviews.classList.replace("show", "hide");
            description.classList.replace("hide", "show");
        }
        else {
            reviews.classList.replace("hide", "show");
            description.classList.replace("show", "hide");
        }
        this.querySelector("span").classList.replace("choice-notchoosen", "choice-choosen");
        this.querySelector("hr").classList.replace("choice-notchoosen-bar", "choice-choosen-bar");
    });
});

document.querySelectorAll(".likes").forEach(element => {
    var likebtn = element.querySelector("button");

    likebtn.addEventListener("click", function () {
        var liketext = element.querySelector("button i");
        var count = element.querySelector("span");
        var previousCount = parseInt(count.textContent);

        var reviewId = this.getAttribute("data-review-id");
        var userId = this.getAttribute("data-user-id");

        $.ajax({
            url: `/Review/UpdateLikes`,
            type: "POST",
            data: {
                reviewId: reviewId,
                userId: userId
            },
            success: function (data) {
                var newCount = data.likes;
                count.textContent = newCount;
                if (newCount > previousCount) {
                    liketext.classList.replace("notliked", "liked");
                } else {
                    liketext.classList.replace("liked", "notliked");
                }
            },
            error: function (xhr) {
                if (xhr.status === 401) {
                    window.location.href = "/Account/Login";
                } else {
                    console.log(xhr.responseText);
                }
            }
        });
    });
});
function initAddToCartBtn(productId, stock, productName) {
    const quantityInput = document.querySelector(".product-btns input");
    const quantity = parseInt(quantityInput.value, 10) || 1; 

    const container = document.querySelector(".addedcheck");
    const addedcheckmsg = document.querySelector(".addedcheck #msggg");
    console.log(`${productId}${stock}${productName}`);

    console.log(container);
    console.log(addedcheckmsg);


    if (quantity > stock) {
        addedcheckmsg.textContent = `"${productName}" Quantity Exceeded Stock (${stock}), Product Not Added!`;
        container.style.backgroundColor = "red";
    } else {
        addedcheckmsg.textContent = `"${productName}" has been added to your cart`;
        container.style.backgroundColor = "#0D975B";

        fetch(`/Cart/AddToCart?productId=${productId}&quantity=${quantity}`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-Requested-With": "XMLHttpRequest"
            }
        })
            .then(response => response.json())
            .then(data => {
            })
            .catch(error => console.error("Error:", error));
    }
}
