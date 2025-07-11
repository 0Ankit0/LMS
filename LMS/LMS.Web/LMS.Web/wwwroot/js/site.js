// Global error handling for JavaScript errors
window.addEventListener('error', function (event) {
    console.error('Global JavaScript Error:', {
        message: event.message,
        filename: event.filename,
        lineno: event.lineno,
        colno: event.colno,
        error: event.error
    });

    // Display error details in the console
    if (event.error && event.error.stack) {
        console.error('Stack trace:', event.error.stack);
    }
});

// Global unhandled promise rejection handler
window.addEventListener('unhandledrejection', function (event) {
    console.error('Unhandled Promise Rejection:', event.reason);
    console.error('Promise:', event.promise);
});

// Enhanced Blazor error handling
window.blazorErrorHandler = {
    onError: function (error) {
        console.error('Blazor Error:', error);
        return false; // Let Blazor handle it normally
    }
};

window.renderQrCode = (elementId, qrCodeUri) => {
    try {
        const container = document.getElementById(elementId);
        if (!container) {
            console.warn('QR Code container not found:', elementId);
            return;
        }
        container.innerHTML = ""; // Clear previous QR code if any
        new QRCode(container, {
            text: qrCodeUri,
            width: 200,
            height: 200
        });
    } catch (error) {
        console.error('Error rendering QR code:', error);
    }
};

window.downloadFileFromBytes = (fileName, contentType, bytes) => {
    try {
        // Convert .NET byte[] (Uint8Array) to a Blob and trigger download
        const blob = new Blob([new Uint8Array(bytes)], { type: contentType });
        const url = URL.createObjectURL(blob);

        const anchor = document.createElement("a");
        anchor.href = url;
        anchor.download = fileName;
        document.body.appendChild(anchor);
        anchor.click();
        document.body.removeChild(anchor);

        setTimeout(() => URL.revokeObjectURL(url), 1000);
    } catch (error) {
        console.error('Error downloading file:', error);
    }
};

// Toast notification system
window.showToast = (message, type = 'info', duration = 5000) => {
    try {
        // Create toast container if it doesn't exist
        let toastContainer = document.getElementById('toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.id = 'toast-container';
            toastContainer.style.cssText = `
                position: fixed;
                top: 20px;
                right: 20px;
                z-index: 9999;
                display: flex;
                flex-direction: column;
                gap: 10px;
                pointer-events: none;
            `;
            document.body.appendChild(toastContainer);
        }

        // Create toast element
        const toast = document.createElement('div');
        toast.style.cssText = `
            background: ${type === 'success' ? '#28a745' : type === 'error' ? '#dc3545' : type === 'warning' ? '#ffc107' : '#17a2b8'};
            color: ${type === 'warning' ? '#212529' : '#fff'};
            padding: 12px 16px;
            border-radius: 8px;
            box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
            max-width: 300px;
            word-wrap: break-word;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            font-size: 14px;
            line-height: 1.4;
            pointer-events: auto;
            transform: translateX(100%);
            opacity: 0;
            transition: all 0.3s ease;
            cursor: pointer;
            position: relative;
        `;

        // Add icon based on type
        const icons = {
            success: '✓',
            error: '✗',
            warning: '⚠',
            info: 'ℹ'
        };

        const icon = icons[type] || icons.info;
        toast.innerHTML = `
            <div style="display: flex; align-items: center; gap: 8px;">
                <span style="font-weight: bold; font-size: 16px;">${icon}</span>
                <span>${message}</span>
            </div>
        `;

        // Add toast to container
        toastContainer.appendChild(toast);

        // Animate in
        requestAnimationFrame(() => {
            toast.style.transform = 'translateX(0)';
            toast.style.opacity = '1';
        });

        // Auto remove after duration
        const removeToast = () => {
            toast.style.transform = 'translateX(100%)';
            toast.style.opacity = '0';
            setTimeout(() => {
                if (toast.parentElement) {
                    toast.parentElement.removeChild(toast);
                }
                // Clean up container if empty
                if (toastContainer.children.length === 0) {
                    document.body.removeChild(toastContainer);
                }
            }, 300);
        };

        // Click to dismiss
        toast.addEventListener('click', removeToast);

        // Auto dismiss
        setTimeout(removeToast, duration);

    } catch (error) {
        console.error('Error showing toast:', error);
        // Fallback to alert
        alert(message);
    }
};