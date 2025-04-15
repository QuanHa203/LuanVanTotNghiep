const accessoryOptions = document.querySelectorAll(".accessory-option");
const container = document.getElementById("selected-accessory-container");

document.addEventListener("DOMContentLoaded", () => {
    const inputCheckboxes = document.querySelectorAll('.accessory-option .accessory-is-select');
    const inputNumbers = document.querySelectorAll('.accessory-option .accessory-quantity');

    inputCheckboxes.forEach(input => {
        inputCheckboxChange(input);
    });

    inputNumbers.forEach(input => {
        inputNumberChange(input);
    });
});

function inputCheckboxChange(input) {
    const box = input.parentElement;
    const accessoryIdElement = box.querySelector(".accessory-id");
    const accessoryQuantityElement = box.querySelector(".accessory-quantity");
    const accessoryNameElement = box.querySelector(".accessory-name");


    if (input.checked == true) {
        const labelElement = document.createElement("label");
        labelElement.id = `accessory-${accessoryIdElement.value}`;
        labelElement.innerHTML = `${accessoryNameElement.value} - Số lượng: ${accessoryQuantityElement.value}`;
        container.appendChild(labelElement);
    }
    else {
        const labelElement = container.querySelector(`#accessory-${accessoryIdElement.value}`);
        if (labelElement)
            container.removeChild(labelElement);
    }

}

function inputNumberChange(input) {
    const box = input.parentElement;
    const accessoryIdElement = box.querySelector(".accessory-id");
    const accessoryQuantityElement = box.querySelector(".accessory-quantity");
    const accessoryNameElement = box.querySelector(".accessory-name");

    const labelElement = container.querySelector(`#accessory-${accessoryIdElement.value}`)
    if (labelElement)
        labelElement.innerHTML = `${accessoryNameElement.value} - Số lượng: ${accessoryQuantityElement.value}`;
}