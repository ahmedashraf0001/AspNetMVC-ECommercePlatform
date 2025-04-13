document.querySelectorAll(".list").forEach(list => {
    const toggleBtn = list.querySelector(".categories-header i");
    const dropdownList = list.querySelector(".dropdown-list");

    toggleBtn.addEventListener("click", function () {
        if (dropdownList.classList.contains("expanded")) {
            dropdownList.classList.remove("expanded");
            dropdownList.style.animation = "hided 0.65s ease-in-out forwards";
        } else {
            dropdownList.classList.add("expanded");
            dropdownList.style.animation = "expand 0.65s ease-in-out forwards";
        }

        this.classList.toggle("rotated");
    });
});