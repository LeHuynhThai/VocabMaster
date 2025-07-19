// Khởi tạo khi trang load
document.addEventListener('DOMContentLoaded', function() {
    loadLearnedWords();
    initializeSearch();
});

// Tải danh sách từ đã học
function loadLearnedWords() {
    fetch('/WordGenerator/GetLearnedWords')
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                updateLearnedWordsList(data.words);
                updateStatistics(data.words);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('error', 'Không thể tải danh sách từ đã học');
        });
}

// Cập nhật danh sách từ đã học
function updateLearnedWordsList(words) {
    const container = document.getElementById('learnedWordsContainer');
    container.innerHTML = '';
    const template = document.getElementById('learnedWordTemplate');

    words.forEach(word => {
        const clone = template.content.cloneNode(true);
        clone.querySelector('.word-text').textContent = word;
        
        // Gắn sự kiện cho các nút
        clone.querySelector('.review-btn').onclick = () => reviewWord(word);
        clone.querySelector('.remove-btn').onclick = () => removeLearnedWord(word);
        
        container.appendChild(clone);
    });
}

// Cập nhật thống kê
function updateStatistics(words) {
    document.getElementById('totalLearned').textContent = words.length;
    // Tính số từ học hôm nay (cần thêm logic phía backend)
    document.getElementById('todayLearned').textContent = '0';
}

// Đánh dấu từ đã học
function markAsLearned(word) {
    if (!word) {
        showToast('error', 'Vui lòng chọn một từ để đánh dấu');
        return;
    }

    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    
    fetch('/WordGenerator/MarkAsLearned', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ word: word })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showToast('success', data.message);
            loadLearnedWords(); // Tải lại danh sách
        } else {
            showToast('error', data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('error', 'Đã xảy ra lỗi. Vui lòng thử lại.');
    });
}

// Tìm kiếm từ đã học với debounce
function initializeSearch() {
    const searchInput = document.getElementById('searchLearned');
    let timeoutId;

    searchInput.addEventListener('input', (e) => {
        clearTimeout(timeoutId);
        timeoutId = setTimeout(() => {
            const searchTerm = e.target.value.toLowerCase();
            const items = document.querySelectorAll('.learned-word-item');
            
            items.forEach(item => {
                const word = item.querySelector('.word-text').textContent.toLowerCase();
                item.style.display = word.includes(searchTerm) ? '' : 'none';
            });
        }, 300);
    });
}

// Hiển thị thông báo
function showToast(type, message) {
    // Tạo toast container nếu chưa tồn tại
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }

    // Tạo toast mới
    const toastId = 'toast-' + Date.now();
    const toast = document.createElement('div');
    toast.className = `toast align-items-center ${type === 'success' ? 'bg-success' : 'bg-danger'} text-white`;
    toast.id = toastId;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    toastContainer.appendChild(toast);

    // Hiển thị toast
    const bsToast = new bootstrap.Toast(toast, { delay: 3000 });
    bsToast.show();

    // Xóa toast sau khi ẩn
    toast.addEventListener('hidden.bs.toast', () => {
        toast.remove();
    });
}

// Xem lại từ
function reviewWord(word) {
    fetch(`/WordGenerator/GetWordDefinition?word=${encodeURIComponent(word)}`, {
        method: 'POST'
    })
    .then(response => {
        if (response.ok) {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        } else {
            showToast('error', 'Không thể tải thông tin từ vựng');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showToast('error', 'Đã xảy ra lỗi khi tải thông tin từ vựng');
    });
}

// Xóa từ đã học
function removeLearnedWord(word) {
    if (confirm(`Bạn có chắc muốn xóa từ "${word}" khỏi danh sách đã học?`)) {
        fetch(`/WordGenerator/RemoveLearnedWord?word=${encodeURIComponent(word)}`, {
            method: 'POST',
            headers: {
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            }
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                showToast('success', 'Đã xóa từ khỏi danh sách');
                loadLearnedWords();
            } else {
                showToast('error', data.message || 'Không thể xóa từ');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showToast('error', 'Đã xảy ra lỗi khi xóa từ');
        });
    }
}