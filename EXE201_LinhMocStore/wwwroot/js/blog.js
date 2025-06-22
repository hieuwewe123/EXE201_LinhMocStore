// Blog JavaScript - Linh Mộc Store

document.addEventListener('DOMContentLoaded', function() {
    // Initialize blog functionality
    initializeBlogFeatures();
});

function initializeBlogFeatures() {
    // Initialize search functionality
    initializeSearch();
    
    // Initialize animations
    initializeAnimations();
    
    // Initialize newsletter form
    initializeNewsletterForm();
    
    // Initialize social sharing
    initializeSocialSharing();
}

// Search functionality
function initializeSearch() {
    const searchInput = document.querySelector('.search-input');
    if (searchInput) {
        let searchTimeout;
        
        searchInput.addEventListener('input', function() {
            clearTimeout(searchTimeout);
            searchTimeout = setTimeout(() => {
                // Auto-submit search after 500ms delay
                if (this.value.length >= 2 || this.value.length === 0) {
                    this.closest('form').submit();
                }
            }, 500);
        });
    }
}

// Animation initialization
function initializeAnimations() {
    // Intersection Observer for fade-in animations
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate__animated', 'animate__fadeInUp');
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Observe blog cards and other elements
    document.querySelectorAll('.blog-card, .blog-filters, .pagination-container').forEach(el => {
        observer.observe(el);
    });

    // Stagger animation for blog cards
    const blogCards = document.querySelectorAll('.blog-card');
    blogCards.forEach((card, index) => {
        card.style.animationDelay = `${index * 0.1}s`;
    });
}

// Newsletter form handling
function initializeNewsletterForm() {
    const newsletterForm = document.querySelector('.newsletter-form');
    if (newsletterForm) {
        newsletterForm.addEventListener('submit', function(e) {
            e.preventDefault();
            
            const emailInput = this.querySelector('input[type="email"]');
            const email = emailInput.value.trim();
            
            if (validateEmail(email)) {
                // Show success message
                showNotification('Đăng ký thành công! Cảm ơn bạn đã đăng ký nhận tin.', 'success');
                emailInput.value = '';
            } else {
                showNotification('Vui lòng nhập email hợp lệ.', 'error');
            }
        });
    }
}

// Social sharing functionality
function initializeSocialSharing() {
    const shareButtons = document.querySelectorAll('.share-btn');
    
    shareButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            // Add click tracking if needed
            console.log('Shared via:', this.classList.contains('facebook') ? 'Facebook' : 
                       this.classList.contains('twitter') ? 'Twitter' : 
                       this.classList.contains('linkedin') ? 'LinkedIn' : 'Zalo');
        });
    });
}

// Email validation
function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

// Notification system
function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'}"></i>
            <span>${message}</span>
            <button class="notification-close">
                <i class="fas fa-times"></i>
            </button>
        </div>
    `;
    
    // Add styles
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        background: ${type === 'success' ? '#10b981' : type === 'error' ? '#ef4444' : '#3b82f6'};
        color: white;
        padding: 15px 20px;
        border-radius: 10px;
        box-shadow: 0 10px 25px rgba(0,0,0,0.2);
        z-index: 10000;
        transform: translateX(100%);
        transition: transform 0.3s ease;
        max-width: 400px;
    `;
    
    // Add to page
    document.body.appendChild(notification);
    
    // Animate in
    setTimeout(() => {
        notification.style.transform = 'translateX(0)';
    }, 100);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        removeNotification(notification);
    }, 5000);
    
    // Close button functionality
    const closeBtn = notification.querySelector('.notification-close');
    closeBtn.addEventListener('click', () => {
        removeNotification(notification);
    });
}

function removeNotification(notification) {
    notification.style.transform = 'translateX(100%)';
    setTimeout(() => {
        if (notification.parentNode) {
            notification.parentNode.removeChild(notification);
        }
    }, 300);
}

// Blog filter functionality
function filterBlogs(filter) {
    const currentUrl = new URL(window.location);
    currentUrl.searchParams.set('filter', filter);
    currentUrl.searchParams.delete('page'); // Reset to first page when filtering
    
    window.location.href = currentUrl.toString();
}

// Reading progress tracking
function initializeReadingProgress() {
    const progressBar = document.getElementById('readingProgress');
    if (progressBar) {
        window.addEventListener('scroll', () => {
            const winScroll = document.body.scrollTop || document.documentElement.scrollTop;
            const height = document.documentElement.scrollHeight - document.documentElement.clientHeight;
            const scrolled = (winScroll / height) * 100;
            progressBar.style.width = scrolled + '%';
        });
    }
}

// Table of Contents functionality
function initializeTableOfContents() {
    const content = document.querySelector('.blog-body');
    const tocList = document.getElementById('tocList');
    
    if (content && tocList) {
        const headings = content.querySelectorAll('h2, h3, h4');
        
        headings.forEach((heading, index) => {
            // Add ID to heading
            heading.id = `heading-${index}`;
            
            // Create TOC item
            const li = document.createElement('li');
            const a = document.createElement('a');
            a.href = `#heading-${index}`;
            a.textContent = heading.textContent;
            a.className = `toc-${heading.tagName.toLowerCase()}`;
            
            li.appendChild(a);
            tocList.appendChild(li);
        });
        
        // Smooth scrolling for TOC links
        tocList.querySelectorAll('a').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const targetId = link.getAttribute('href');
                const targetElement = document.querySelector(targetId);
                
                if (targetElement) {
                    targetElement.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });
        
        // Active TOC highlighting
        window.addEventListener('scroll', () => {
            const headings = content.querySelectorAll('h2, h3, h4');
            const tocLinks = tocList.querySelectorAll('a');
            
            let current = '';
            
            headings.forEach((heading, index) => {
                const rect = heading.getBoundingClientRect();
                if (rect.top <= 100) {
                    current = `#heading-${index}`;
                }
            });
            
            tocLinks.forEach(link => {
                link.classList.remove('active');
                if (link.getAttribute('href') === current) {
                    link.classList.add('active');
                }
            });
        });
    }
}

// Lazy loading for images
function initializeLazyLoading() {
    const images = document.querySelectorAll('img[data-src]');
    
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                imageObserver.unobserve(img);
            }
        });
    });
    
    images.forEach(img => imageObserver.observe(img));
}

// Initialize specific features based on page
if (document.querySelector('.blog-detail-hero')) {
    // Blog detail page
    initializeReadingProgress();
    initializeTableOfContents();
} else if (document.querySelector('.blog-hero')) {
    // Blog index page
    initializeLazyLoading();
}

// Export functions for global use
window.blogUtils = {
    filterBlogs,
    showNotification,
    validateEmail
}; 