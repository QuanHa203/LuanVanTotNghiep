function showAndHidePassword(iconElement) {
    let parentElement = iconElement.parentElement;
    let inputElement = parentElement.querySelector("input");

    if (iconElement.classList.contains("eye-hide-icon")) {
        iconElement.classList.remove("eye-hide-icon")
        iconElement.classList.add("eye-icon")
        inputElement.type = "password";
    }
    else {
        iconElement.classList.remove("eye-icon")
        iconElement.classList.add("eye-hide-icon")
        inputElement.type = "text";
    }

}