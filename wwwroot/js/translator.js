// Translator Page JavaScript

// Global variables
let selectedFile = null;
let translationInProgress = false;
let currentProgress = 0;

// DOM elements
const uploadArea = document.getElementById('uploadArea');
const movieFile = document.getElementById('movieFile');
const downloadBtn = document.getElementById('downloadBtn');
const translateBtn = document.getElementById('translateBtn');
const statusSection = document.getElementById('statusSection');
const progressFill = document.getElementById('progressFill');
const progressText = document.getElementById('progressText');
const statusMessage = document.getElementById('statusMessage');
const loadingOverlay = document.getElementById('loadingOverlay');
const adminNotes = document.getElementById('adminNotes');

// Initialize page
document.addEventListener('DOMContentLoaded', function () {
    setupFileUpload();
    loadSavedNotes();
});

// File upload setup
function setupFileUpload() {
    // Drag and drop functionality
    uploadArea.addEventListener('dragover', handleDragOver);
    uploadArea.addEventListener('dragleave', handleDragLeave);
    uploadArea.addEventListener('drop', handleDrop);
    uploadArea.addEventListener('click', () => movieFile.click());

    // File input change
    movieFile.addEventListener('change', handleFileSelect);
}

function handleDragOver(e) {
    e.preventDefault();
    uploadArea.classList.add('drag-over');
}

function handleDragLeave(e) {
    e.preventDefault();
    uploadArea.classList.remove('drag-over');
}

function handleDrop(e) {
    e.preventDefault();
    uploadArea.classList.remove('drag-over');

    const files = e.dataTransfer.files;
    if (files.length > 0) {
        handleFileSelection(files[0]);
    }
}

function handleFileSelect(e) {
    if (e.target.files.length > 0) {
        handleFileSelection(e.target.files[0]);
    }
}

function handleFileSelection(file) {
    // Validate file type
    const allowedTypes = ['video/mp4', 'video/avi', 'video/x-msvideo', 'video/quicktime', 'video/x-matroska'];
    const allowedExtensions = ['.mp4', '.avi', '.mkv', '.mov'];

    const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));

    if (!allowedTypes.includes(file.type) && !allowedExtensions.includes(fileExtension)) {
        showNotification('Please select a valid video file (MP4, AVI, MKV, MOV)', 'error');
        return;
    }

    // Check file size (limit to 500MB for demo)
    const maxSize = 500 * 1024 * 1024; // 500MB
    if (file.size > maxSize) {
        showNotification('File size should be less than 500MB', 'error');
        return;
    }

    selectedFile = file;
    updateUploadArea();
    enableTranslationControls();
}

function updateUploadArea() {
    if (selectedFile) {
        const uploadContent = uploadArea.querySelector('.upload-content');
        uploadContent.innerHTML = `
            <div class="upload-icon">✅</div>
            <p class="upload-text">File Selected: ${selectedFile.name}</p>
            <p class="file-info">Size: ${formatFileSize(selectedFile.size)}</p>
            <button class="upload-btn" onclick="document.getElementById('movieFile').click()">
                Change File
            </button>
        `;
    }
}

function enableTranslationControls() {
    translateBtn.disabled = false;
    translateBtn.style.opacity = '1';
}

function formatFileSize(bytes) {
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 Bytes';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
}

// Translation functions
function startTranslation() {
    if (!selectedFile) {
        showNotification('Please select a file first', 'error');
        return;
    }

    if (translationInProgress) {
        showNotification('Translation already in progress', 'warning');
        return;
    }

    const sourceLanguage = document.getElementById('sourceLanguage').value;
    const targetLanguage = document.getElementById('targetLanguage').value;
    const translationMode = document.getElementById('translationMode').value;

    if (sourceLanguage === targetLanguage) {
        showNotification('Source and target languages cannot be the same', 'error');
        return;
    }

    // Start translation process
    translationInProgress = true;
    showStatusSection();
    updateTranslationStatus('Initializing translation...', 0);

    // Disable controls during translation
    translateBtn.disabled = true;
    translateBtn.textContent = '🔄 Translating...';

    // Simulate translation process
    simulateTranslation();
}

function simulateTranslation() {
    const steps = [
        { message: 'Analyzing video file...', progress: 10 },
        { message: 'Extracting audio track...', progress: 25 },
        { message: 'Converting speech to text...', progress: 45 },
        { message: 'Translating content with AI...', progress: 70 },
        { message: 'Generating subtitles...', progress: 85 },
        { message: 'Finalizing translation...', progress: 95 },
        { message: 'Translation completed successfully!', progress: 100 }
    ];

    let currentStep = 0;

    const interval = setInterval(() => {
        if (currentStep < steps.length) {
            const step = steps[currentStep];
            updateTranslationStatus(step.message, step.progress);
            currentStep++;
        } else {
            clearInterval(interval);
            completeTranslation();
        }
    }, 2000);
}

function updateTranslationStatus(message, progress) {
    statusMessage.textContent = message;
    progressFill.style.width = progress + '%';
    progressText.textContent = progress + '%';
    currentProgress = progress;
}

function completeTranslation() {
    translationInProgress = false;
    translateBtn.disabled = false;
    translateBtn.textContent = '🚀 Start Translation';

    // Enable download button
    downloadBtn.disabled = false;
    downloadBtn.style.opacity = '1';

    showNotification('Translation completed successfully!', 'success');
}

function resetTranslation() {
    if (translationInProgress) {
        if (!confirm('Are you sure you want to cancel the current translation?')) {
            return;
        }
    }

    // Reset all states
    selectedFile = null;
    translationInProgress = false;
    currentProgress = 0;

    // Reset UI
    const uploadContent = uploadArea.querySelector('.upload-content');
    uploadContent.innerHTML = `
        <div class="upload-icon">📁</div>
        <p class="upload-text">Input movie file</p>
        <input type="file" id="movieFile" class="file-input" accept=".mp4,.avi,.mkv,.mov" />
        <button class="upload-btn" onclick="document.getElementById('movieFile').click()">
            Choose File
        </button>
    `;

    // Reset form
    document.getElementById('sourceLanguage').value = 'auto';
    document.getElementById('targetLanguage').value = 'vi';
    document.getElementById('translationMode').value = 'subtitle';

    // Reset buttons
    translateBtn.disabled = true;
    translateBtn.textContent = '🚀 Start Translation';
    translateBtn.style.opacity = '0.6';

    downloadBtn.disabled = true;
    downloadBtn.style.opacity = '0.6';

    // Hide status section
    hideStatusSection();

    // Re-setup file upload
    setupFileUpload();

    showNotification('Translation reset successfully', 'info');
}

function showStatusSection() {
    statusSection.style.display = 'block';
    statusSection.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

function hideStatusSection() {
    statusSection.style.display = 'none';
}

// Notes functions
function saveNotes() {
    const notes = adminNotes.value.trim();

    if (notes) {
        localStorage.setItem('adminTranslationNotes', notes);
        showNotification('Notes saved successfully', 'success');
    } else {
        showNotification('Please enter some notes to save', 'warning');
    }
}

function clearNotes() {
    if (adminNotes.value.trim() && !confirm('Are you sure you want to clear all notes?')) {
        return;
    }

    adminNotes.value = '';
    localStorage.removeItem('adminTranslationNotes');
    showNotification('Notes cleared', 'info');
}

function loadSavedNotes() {
    const savedNotes = localStorage.getItem('adminTranslationNotes');
    if (savedNotes) {
        adminNotes.value = savedNotes;
    }
}

// Utility functions
function showNotification(message, type = 'info') {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.notification');
    existingNotifications.forEach(notif => notif.remove());

    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <span class="notification-message">${message}</span>
        <button class="notification-close" onclick="this.parentElement.remove()">×</button>
    `;

    // Add notification styles
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${getNotificationColor(type)};
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 8px;
        display: flex;
        align-items: center;
        gap: 1rem;
        z-index: 10000;
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.3);
        animation: slideIn 0.3s ease;
    `;

    document.body.appendChild(notification);

    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);
}

function getNotificationColor(type) {
    const colors = {
        success: '#28a745',
        error: '#dc3545',
        warning: '#ffc107',
        info: '#17a2b8'
    };
    return colors[type] || colors.info;
}

// Navigation functions
function toggleNotifications() {
    showNotification('Notifications feature coming soon!', 'info');
}

function toggleProfile() {
    showNotification('Profile settings coming soon!', 'info');
}

// Download function
downloadBtn.addEventListener('click', function () {
    if (!this.disabled) {
        // In a real implementation, this would download the translated file
        showNotification('Download started! Check your downloads folder.', 'success');

        // Simulate download
        const link = document.createElement('a');
        link.href = '#';
        link.download = `translated_${selectedFile ? selectedFile.name : 'movie'}.srt`;
        link.textContent = 'Download Subtitles';

        // For demo purposes, we'll just show a message
        console.log('Download would start here for:', link.download);
    }
});

// Add CSS for animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    .drag-over {
        border-color: #667eea !important;
        background: rgba(102, 126, 234, 0.1) !important;
        transform: scale(1.02);
    }
    
    .notification-close {
        background: transparent;
        border: none;
        color: white;
        font-size: 1.2rem;
        cursor: pointer;
        padding: 0;
        width: 20px;
        height: 20px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 50%;
        transition: background 0.2s ease;
    }
    
    .notification-close:hover {
        background: rgba(255, 255, 255, 0.2);
    }
    
    .file-info {
        color: #888;
        font-size: 0.9rem;
        margin: 0.5rem 0;
    }
`;
document.head.appendChild(style);