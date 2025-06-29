// Global variables
let uploadedMovieFile = null;
let uploadedSubtitleFile = null;
let originalSubtitleText = '';
let translatedSubtitleText = '';
let isTranslating = false;

// DOM elements
const movieUploadArea = document.getElementById('movieUploadArea');
const movieFileInput = document.getElementById('movieFileInput');
const originalSubtitleUpload = document.getElementById('originalSubtitleUpload');
const originalSubtitleInput = document.getElementById('originalSubtitleInput');
const originalTextarea = document.getElementById('originalTextarea');
const translatedTextarea = document.getElementById('translatedTextarea');
const translationPlaceholder = document.getElementById('translationPlaceholder');
const sourceLanguageSelect = document.getElementById('sourceLanguage');
const targetLanguageSelect = document.getElementById('targetLanguage');
const swapLanguagesBtn = document.getElementById('swapLanguages');
const translateBtn = document.getElementById('translateBtn');
const clearAllBtn = document.getElementById('clearAllBtn');
const saveEditsBtn = document.getElementById('saveEditsBtn');
const autoFormatBtn = document.getElementById('autoFormatBtn');
const downloadPlaceholder = document.getElementById('downloadPlaceholder');
const downloadReadyBtn = document.getElementById('downloadReadyBtn');
const adminNotes = document.getElementById('adminNotes');
const saveNotesBtn = document.getElementById('saveNotesBtn');
const clearNotesBtn = document.getElementById('clearNotesBtn');
const notificationBtn = document.getElementById('notificationBtn');
const notificationModal = document.getElementById('notificationModal');
const loadingModal = document.getElementById('loadingModal');
const loadingText = document.getElementById('loadingText');
const originalLanguageTag = document.getElementById('originalLanguageTag');
const translatedLanguageTag = document.getElementById('translatedLanguageTag');

// Language names mapping
const languageNames = {
    'en': 'English',
    'vi': 'Vietnamese',
    'ja': 'Japanese',
    'ko': 'Korean',
    'zh': 'Chinese',
    'es': 'Spanish',
    'fr': 'French',
    'de': 'German'
};

// Initialize the application
document.addEventListener('DOMContentLoaded', function () {
    initializeEventListeners();
    initializeDragAndDrop();
    updateLanguageTags();
    loadSavedNotes();
});

// Initialize event listeners
function initializeEventListeners() {
    // File inputs
    movieFileInput.addEventListener('change', handleMovieFileSelect);
    originalSubtitleInput.addEventListener('change', handleSubtitleFileSelect);

    // Language controls
    sourceLanguageSelect.addEventListener('change', updateLanguageTags);
    targetLanguageSelect.addEventListener('change', updateLanguageTags);
    swapLanguagesBtn.addEventListener('click', swapLanguages);

    // Translation controls
    translateBtn.addEventListener('click', startTranslation);
    clearAllBtn.addEventListener('click', clearAll);

    // Translation actions
    saveEditsBtn.addEventListener('click', saveEdits);
    autoFormatBtn.addEventListener('click', autoFormat);
    downloadReadyBtn.addEventListener('click', downloadTranslatedFile);

    // Admin notes
    saveNotesBtn.addEventListener('click', saveNotes);
    clearNotesBtn.addEventListener('click', clearNotes);

    // Notifications
    notificationBtn.addEventListener('click', toggleNotifications);

    // Text area changes
    translatedTextarea.addEventListener('input', onTranslatedTextChange);
}

// Initialize drag and drop functionality
function initializeDragAndDrop() {
    // Movie file drag and drop
    setupDragAndDrop(movieUploadArea, handleMovieFilesDrop, ['video/mp4', 'video/avi', 'video/x-msvideo', 'video/quicktime']);

    // Subtitle file drag and drop
    setupDragAndDrop(originalSubtitleUpload, handleSubtitleFilesDrop, ['text/plain', 'application/x-subrip']);
}

// Setup drag and drop for an element
function setupDragAndDrop(element, dropHandler, allowedTypes) {
    element.addEventListener('dragover', function (e) {
        e.preventDefault();
        element.classList.add('drag-over');
    });

    element.addEventListener('dragleave', function (e) {
        e.preventDefault();
        element.classList.remove('drag-over');
    });

    element.addEventListener('drop', function (e) {
        e.preventDefault();
        element.classList.remove('drag-over');
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            dropHandler(files[0], allowedTypes);
        }
    });
}

// Handle movie file selection
function handleMovieFileSelect(event) {
    const file = event.target.files[0];
    if (file) {
        handleMovieFilesDrop(file, ['video/mp4', 'video/avi', 'video/x-msvideo', 'video/quicktime']);
    }
}

// Handle movie file drop
function handleMovieFilesDrop(file, allowedTypes) {
    if (!validateFileType(file, allowedTypes, ['.mp4', '.avi', '.mkv', '.mov', '.wmv'])) {
        showNotification('Please select a valid video file (MP4, AVI, MKV, MOV, WMV)', 'error');
        return;
    }

    if (file.size > 2147483648) { // 2GB limit
        showNotification('File size too large. Maximum size is 2GB.', 'error');
        return;
    }

    uploadedMovieFile = file;
    updateMovieUploadDisplay();
    uploadMovieFile(file);
}

// Handle subtitle file selection
function handleSubtitleFileSelect(event) {
    const file = event.target.files[0];
    if (file) {
        handleSubtitleFilesDrop(file, ['text/plain']);
    }
}

// Handle subtitle file drop
function handleSubtitleFilesDrop(file, allowedTypes) {
    if (!validateFileType(file, allowedTypes, ['.srt', '.vtt', '.ass', '.ssa', '.sub'])) {
        showNotification('Please select a valid subtitle file (SRT, VTT, ASS, SSA, SUB)', 'error');
        return;
    }

    uploadedSubtitleFile = file;
    updateSubtitleUploadDisplay();
    readSubtitleFile(file);
}

// Validate file type
function validateFileType(file, allowedMimeTypes, allowedExtensions) {
    const fileExtension = file.name.toLowerCase().substring(file.name.lastIndexOf('.'));
    return allowedMimeTypes.includes(file.type) || allowedExtensions.includes(fileExtension);
}

// Update movie upload display
function updateMovieUploadDisplay() {
    const uploadContent = movieUploadArea.querySelector('.upload-placeholder');
    uploadContent.innerHTML = `
        <i class="fas fa-check-circle" style="color: #28a745;"></i>
        <p style="color: #28a745; font-weight: 600;">Movie File Uploaded</p>
        <p style="font-size: 0.9rem; color: #6c757d;">${uploadedMovieFile.name}</p>
        <p style="font-size: 0.8rem; color: #6c757d;">${formatFileSize(uploadedMovieFile.size)}</p>
        <button class="upload-btn" onclick="document.getElementById('movieFileInput').click()">
            Change File
        </button>
    `;
    movieUploadArea.classList.add('file-uploaded');
}

// Update subtitle upload display
function updateSubtitleUploadDisplay() {
    const uploadContent = originalSubtitleUpload.querySelector('.upload-placeholder');
    uploadContent.innerHTML = `
        <i class="fas fa-check-circle" style="color: #28a745;"></i>
        <p style="color: #28a745; font-weight: 600;">Subtitle Uploaded</p>
        <p style="font-size: 0.9rem; color: #6c757d;">${uploadedSubtitleFile.name}</p>
        <button class="upload-btn" onclick="document.getElementById('originalSubtitleInput').click()">
            Change File
        </button>
    `;
    originalSubtitleUpload.classList.add('file-uploaded');
}

// Upload movie file to server (mock implementation)
async function uploadMovieFile(file) {
    try {
        // Mock server upload - replace with actual endpoint
        console.log('Uploading movie file:', file.name);
        showNotification('Movie file uploaded successfully', 'success');
    } catch (error) {
        console.error('Error uploading movie file:', error);
        showNotification('Failed to upload movie file', 'error');
    }
}

// Read subtitle file content
function readSubtitleFile(file) {
    const reader = new FileReader();
    reader.onload = function (e) {
        originalSubtitleText = e.target.result;
        originalTextarea.value = originalSubtitleText;
        showNotification('Subtitle file loaded successfully', 'success');
    };
    reader.readAsText(file);
}

// Update language tags
function updateLanguageTags() {
    const sourceLang = sourceLanguageSelect.value;
    const targetLang = targetLanguageSelect.value;

    originalLanguageTag.textContent = languageNames[sourceLang] || '1st Language';
    translatedLanguageTag.textContent = languageNames[targetLang] || '2nd Language';
}

// Swap languages
function swapLanguages() {
    const sourceValue = sourceLanguageSelect.value;
    const targetValue = targetLanguageSelect.value;

    sourceLanguageSelect.value = targetValue;
    targetLanguageSelect.value = sourceValue;

    // Swap text content if both exist
    if (originalSubtitleText && translatedSubtitleText) {
        const tempText = originalTextarea.value;
        originalTextarea.value = translatedTextarea.value;
        translatedTextarea.value = tempText;

        originalSubtitleText = originalTextarea.value;
        translatedSubtitleText = translatedTextarea.value;
    }

    updateLanguageTags();
    showNotification('Languages swapped successfully', 'info');
}

// Start translation process
async function startTranslation() {
    if (!originalSubtitleText.trim()) {
        showNotification('Please upload a subtitle file first', 'error');
        return;
    }

    if (sourceLanguageSelect.value === targetLanguageSelect.value) {
        showNotification('Source and target languages cannot be the same', 'error');
        return;
    }

    if (isTranslating) {
        showNotification('Translation is already in progress', 'warning');
        return;
    }

    isTranslating = true;
    showLoadingModal('Translating subtitles...');

    try {
        // Mock translation process - replace with actual API call
        await mockTranslationProcess();

        const translatedText = simulateTranslation(originalSubtitleText, sourceLanguageSelect.value, targetLanguageSelect.value);
        translatedSubtitleText = translatedText;
        showTranslatedText();
        showNotification('Translation completed successfully', 'success');
        enableDownload();
    } catch (error) {
        console.error('Translation error:', error);
        showNotification('Translation failed. Please try again.', 'error');
    } finally {
        isTranslating = false;
        hideLoadingModal();
    }
}

// Mock translation process
async function mockTranslationProcess() {
    const steps = [
        'Analyzing subtitle content...',
        'Processing language patterns...',
        'Applying AI translation...',
        'Optimizing subtitle timing...',
        'Finalizing translation...'
    ];

    for (let i = 0; i < steps.length; i++) {
        loadingText.textContent = steps[i];
        await new Promise(resolve => setTimeout(resolve, 1000));
    }
}

// Simulate translation (replace with real translation API)
function simulateTranslation(originalText, sourceLanguage, targetLanguage) {
    const lines = originalText.split('\n');
    const translatedLines = [];

    for (const line of lines) {
        if (!line.trim()) {
            translatedLines.push(line);
            continue;
        }

        // Keep timestamps and sequence numbers as is
        if (line.includes('-->') || line.match(/^\d+$/)) {
            translatedLines.push(line);
            continue;
        }

        // Mock translation based on target language
        const translatedLine = targetLanguage === 'vi' ? `[VI] ${line}` :
            targetLanguage === 'ja' ? `[JP] ${line}` :
                targetLanguage === 'ko' ? `[KO] ${line}` :
                    targetLanguage === 'zh' ? `[ZH] ${line}` :
                        targetLanguage === 'es' ? `[ES] ${line}` :
                            targetLanguage === 'fr' ? `[FR] ${line}` :
                                targetLanguage === 'de' ? `[DE] ${line}` :
                                    `[EN] ${line}`;

        translatedLines.push(translatedLine);
    }

    return translatedLines.join('\n');
}

// Show translated text
function showTranslatedText() {
    translationPlaceholder.style.display = 'none';
    translatedTextarea.style.display = 'block';
    translatedTextarea.value = translatedSubtitleText;
}

// Handle translated text changes
function onTranslatedTextChange() {
    translatedSubtitleText = translatedTextarea.value;
}

// Save edits
function saveEdits() {
    if (!translatedSubtitleText.trim()) {
        showNotification('No translated text to save', 'warning');
        return;
    }

    translatedSubtitleText = translatedTextarea.value;
    showNotification('Edits saved successfully', 'success');
}

// Auto format subtitle text
function autoFormat() {
    if (!translatedSubtitleText.trim()) {
        showNotification('No text to format', 'warning');
        return;
    }

    // Basic SRT formatting
    let formattedText = translatedSubtitleText
        .replace(/\r\n/g, '\n')
        .replace(/\n{3,}/g, '\n\n')
        .trim();

    translatedTextarea.value = formattedText;
    translatedSubtitleText = formattedText;
    showNotification('Text formatted successfully', 'success');
}

// Enable download
function enableDownload() {
    downloadPlaceholder.style.display = 'none';
    downloadReadyBtn.style.display = 'flex';
}

// Download translated file
function downloadTranslatedFile() {
    if (!translatedSubtitleText.trim()) {
        showNotification('No translated text to download', 'error');
        return;
    }

    try {
        const blob = new Blob([translatedSubtitleText], { type: 'text/plain' });
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `translated_subtitle_${Date.now()}.srt`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);

        showNotification('File downloaded successfully', 'success');
    } catch (error) {
        console.error('Download error:', error);
        showNotification('Download failed', 'error');
    }
}

// Clear all data
function clearAll() {
    if (confirm('Are you sure you want to clear all data? This action cannot be undone.')) {
        // Reset variables
        uploadedMovieFile = null;
        uploadedSubtitleFile = null;
        originalSubtitleText = '';
        translatedSubtitleText = '';

        // Reset UI
        movieUploadArea.innerHTML = `
            <div class="upload-placeholder">
                <i class="fas fa-cloud-upload-alt"></i>
                <p>Input or drag movie file</p>
                <input type="file" id="movieFileInput" accept=".mp4,.avi,.mkv,.mov,.wmv" hidden>
                <button class="upload-btn" onclick="document.getElementById('movieFileInput').click()">
                    Choose File
                </button>
            </div>
        `;

        originalSubtitleUpload.innerHTML = `
            <div class="upload-placeholder">
                <i class="fas fa-file-upload"></i>
                <p>input or drag srt file</p>
                <input type="file" id="originalSubtitleInput" accept=".srt,.vtt,.ass,.ssa,.sub" hidden>
                <button class="upload-btn" onclick="document.getElementById('originalSubtitleInput').click()">
                    Choose File
                </button>
            </div>
        `;

        originalTextarea.value = '';
        translatedTextarea.value = '';
        translatedTextarea.style.display = 'none';
        translationPlaceholder.style.display = 'flex';
        downloadReadyBtn.style.display = 'none';
        downloadPlaceholder.style.display = 'flex';

        // Reset file input references
        const newMovieInput = document.getElementById('movieFileInput');
        const newSubtitleInput = document.getElementById('originalSubtitleInput');
        newMovieInput.addEventListener('change', handleMovieFileSelect);
        newSubtitleInput.addEventListener('change', handleSubtitleFileSelect);

        // Re-initialize drag and drop
        initializeDragAndDrop();

        showNotification('All data cleared', 'info');
    }
}

// Save admin notes
function saveNotes() {
    const note = adminNotes.value.trim();
    if (!note) {
        showNotification('Please enter a note', 'warning');
        return;
    }

    try {
        localStorage.setItem('translatorAdminNotes', note);
        showNotification('Note saved successfully', 'success');
    } catch (error) {
        console.error('Error saving note:', error);
        showNotification('Failed to save note', 'error');
    }
}

// Clear admin notes
function clearNotes() {
    if (confirm('Are you sure you want to clear the note?')) {
        adminNotes.value = '';
        localStorage.removeItem('translatorAdminNotes');
        showNotification('Note cleared', 'info');
    }
}

// Load saved notes
function loadSavedNotes() {
    const savedNotes = localStorage.getItem('translatorAdminNotes');
    if (savedNotes) {
        adminNotes.value = savedNotes;
    }
}

// Toggle notifications
function toggleNotifications() {
    notificationModal.classList.toggle('show');
}

// Close notifications
function closeNotifications() {
    notificationModal.classList.remove('show');
}

// Show loading modal
function showLoadingModal(text) {
    loadingText.textContent = text;
    loadingModal.style.display = 'flex';
}

// Hide loading modal
function hideLoadingModal() {
    loadingModal.style.display = 'none';
}

// Show notification
function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <i class="fas ${getNotificationIcon(type)}"></i>
            <span>${message}</span>
        </div>
    `;

    // Add styles
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${getNotificationColor(type)};
        color: white;
        padding: 1rem 1.5rem;
        border-radius: 6px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.2);
        z-index: 1001;
        animation: slideIn 0.3s ease;
        max-width: 400px;
    `;

    document.body.appendChild(notification);

    // Remove after 4 seconds
    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease';
        setTimeout(() => {
            if (notification.parentNode) {
                notification.parentNode.removeChild(notification);
            }
        }, 300);
    }, 4000);
}

// Get notification icon
function getNotificationIcon(type) {
    switch (type) {
        case 'success': return 'fa-check-circle';
        case 'error': return 'fa-exclamation-circle';
        case 'warning': return 'fa-exclamation-triangle';
        default: return 'fa-info-circle';
    }
}

// Get notification color
function getNotificationColor(type) {
    switch (type) {
        case 'success': return '#28a745';
        case 'error': return '#dc3545';
        case 'warning': return '#ffc107';
        default: return '#17a2b8';
    }
}

// Format file size
function formatFileSize(bytes) {
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 Bytes';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
}

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    
    @keyframes slideOut {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(100%); opacity: 0; }
    }
    
    .drag-over {
        border-color: #007bff !important;
        background: #f8f9ff !important;
    }
`;
document.head.appendChild(style);