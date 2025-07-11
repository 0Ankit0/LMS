// Searchable Select Component JavaScript helpers

window.getScrollInfo = (element) => {
    if (!element) {
        return {
            scrollTop: 0,
            scrollHeight: 0,
            clientHeight: 0
        };
    }

    return {
        scrollTop: element.scrollTop,
        scrollHeight: element.scrollHeight,
        clientHeight: element.clientHeight
    };
};

// Click outside handler for dropdown
window.addClickOutsideListener = (element, dotNetObjectRef) => {
    const clickHandler = (event) => {
        if (!element.contains(event.target)) {
            dotNetObjectRef.invokeMethodAsync('CloseDropdown');
        }
    };

    document.addEventListener('click', clickHandler);

    // Return cleanup function
    return () => {
        document.removeEventListener('click', clickHandler);
    };
};
