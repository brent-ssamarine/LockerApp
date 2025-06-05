window.scrollToElement = (elementId) => {
    const element = document.getElementById(elementId);
    if (element) {
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });
        // Add a temporary highlight effect
        element.style.transition = 'background-color 0.5s';
        element.style.backgroundColor = '#e6f3ff';
        setTimeout(() => {
            element.style.backgroundColor = '';
        }, 2000);
    }
};

// Store blob URLs to revoke them later
window.blobUrls = new Map();

// Function to handle PDF files (open in browser or download)
window.downloadFile = function (filename, contentType, base64Data) {
    if (contentType === 'application/pdf') {
        // Convert base64 to blob
        const byteCharacters = atob(base64Data);
        const byteNumbers = new Array(byteCharacters.length);
        for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        const byteArray = new Uint8Array(byteNumbers);
        const blob = new Blob([byteArray], { type: contentType });

        // Create a temporary file URL
        const fileUrl = URL.createObjectURL(blob);

        // Clean up the URL after 1 minute
        setTimeout(() => {
            URL.revokeObjectURL(fileUrl);
        }, 60000);

        return fileUrl;
    } else {
        // For other files, download as before
        const dataUrl = `data:${contentType};base64,${base64Data}`;
        const link = document.createElement('a');
        link.href = dataUrl;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        return null;
    }
}; 