// Khởi tạo khi trang load
document.addEventListener('DOMContentLoaded', function() {
    console.log('DOM loaded');
});

// Tìm kiếm từ đã học
function searchLearnedWords() {
    const searchTerm = document.getElementById('searchLearned').value.toLowerCase();
    const items = document.querySelectorAll('.learned-word-item');
    
    items.forEach(item => {
        const word = item.querySelector('.word-text').textContent.toLowerCase();
        item.style.display = word.includes(searchTerm) ? '' : 'none';
    });
}
