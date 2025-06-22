// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Site.js - Custom JavaScript for Linh Moc Store

// Global variables
let cartItems = JSON.parse(localStorage.getItem('cartItems')) || [];
let wishlistItems = JSON.parse(localStorage.getItem('wishlistItems')) || [];

// Utility functions
const utils = {
    // Format currency
    formatCurrency: (amount) => {
        return new Intl.NumberFormat('vi-VN', {
            style: 'currency',
            currency: 'VND'
        }).format(amount);
    },

    // Show notification
    showNotification: (message, type = 'success') => {
        const notification = document.createElement('div');
        notification.className = `alert alert-${type} notification`;
        notification.innerHTML = `
            <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>
            ${message}
        `;
        
        document.body.appendChild(notification);
        
        // Animate in
        setTimeout(() => notification.classList.add('show'), 100);
        
        // Remove after 3 seconds
        setTimeout(() => {
            notification.classList.remove('show');
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    },

    // Debounce function
    debounce: (func, wait) => {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    },

    // Smooth scroll to element
    scrollToElement: (element, offset = 0) => {
        const elementPosition = element.offsetTop - offset;
        window.scrollTo({
            top: elementPosition,
            behavior: 'smooth'
        });
    }
};

// Cart functionality
const cart = {
    addItem: (product) => {
        // Kiểm tra đăng nhập
        if (typeof isLoggedIn !== 'undefined' && isLoggedIn !== 'true') {
            window.location.href = '/login?returnUrl=' + encodeURIComponent(window.location.pathname);
            return;
        }
        const existingItem = cartItems.find(item => item.id === product.id);
        // Kiểm tra tồn kho
        if (existingItem) {
            if (existingItem.quantity + 1 > product.stock) {
                utils.showNotification('Số lượng vượt quá số lượng còn lại!', 'danger');
                return;
            }
            existingItem.quantity += 1;
        } else {
            if (product.stock < 1) {
                utils.showNotification('Sản phẩm đã hết hàng!', 'danger');
                return;
            }
            cartItems.push({
                id: product.id,
                name: product.name,
                price: product.price,
                image: product.image,
                quantity: 1,
                stock: product.stock
            });
        }
        cart.saveToStorage();
        cart.updateCartBadge();
        utils.showNotification('Sản phẩm đã được thêm vào giỏ hàng');
    },

    removeItem: (productId) => {
        cartItems = cartItems.filter(item => item.id !== productId);
        cart.saveToStorage();
        cart.updateCartBadge();
        utils.showNotification('Sản phẩm đã được xóa khỏi giỏ hàng');
    },

    updateQuantity: (productId, quantity) => {
        const item = cartItems.find(item => item.id === productId);
        if (item) {
            item.quantity = Math.max(1, quantity);
            cart.saveToStorage();
            cart.updateCartBadge();
        }
    },

    getTotal: () => {
        return cartItems.reduce((total, item) => total + (item.price * item.quantity), 0);
    },

    saveToStorage: () => {
        localStorage.setItem('cartItems', JSON.stringify(cartItems));
    },

    updateCartBadge: () => {
        const badge = document.querySelector('.badge');
        if (badge) {
            const totalItems = cartItems.reduce((total, item) => total + item.quantity, 0);
            badge.textContent = totalItems;
            badge.style.display = totalItems > 0 ? 'block' : 'none';
        }
    },

    clear: () => {
        cartItems = [];
        cart.saveToStorage();
        cart.updateCartBadge();
    }
};

// Wishlist functionality
const wishlist = {
    addItem: (product) => {
        if (!wishlistItems.find(item => item.id === product.id)) {
            wishlistItems.push({
                id: product.id,
                name: product.name,
                price: product.price,
                image: product.image
            });
            wishlist.saveToStorage();
            utils.showNotification('Sản phẩm đã được thêm vào yêu thích');
        }
    },

    removeItem: (productId) => {
        wishlistItems = wishlistItems.filter(item => item.id !== productId);
        wishlist.saveToStorage();
        utils.showNotification('Sản phẩm đã được xóa khỏi yêu thích');
    },

    saveToStorage: () => {
        localStorage.setItem('wishlistItems', JSON.stringify(wishlistItems));
    }
};

// Search functionality
const search = {
    init: () => {
        const searchInput = document.querySelector('#searchInput');
        if (searchInput) {
            searchInput.addEventListener('input', utils.debounce(search.performSearch, 300));
        }
    },

    performSearch: (event) => {
        const query = event.target.value.toLowerCase();
        const productCards = document.querySelectorAll('.product-card');
        
        productCards.forEach(card => {
            const productName = card.querySelector('.card-title').textContent.toLowerCase();
            const productDesc = card.querySelector('.card-text').textContent.toLowerCase();
            
            if (productName.includes(query) || productDesc.includes(query)) {
                card.style.display = 'block';
                card.classList.add('animate__animated', 'animate__fadeIn');
            } else {
                card.style.display = 'none';
            }
        });
    }
};

// Product gallery
const productGallery = {
    init: () => {
        const mainImage = document.querySelector('.product-main-image');
        const thumbnails = document.querySelectorAll('.product-thumbnail');
        
        if (mainImage && thumbnails.length > 0) {
            thumbnails.forEach(thumb => {
                thumb.addEventListener('click', () => {
                    const newSrc = thumb.getAttribute('data-src');
                    mainImage.src = newSrc;
                    
                    // Update active thumbnail
                    thumbnails.forEach(t => t.classList.remove('active'));
                    thumb.classList.add('active');
                });
            });
        }
    }
};

// Lazy loading for images
const lazyLoading = {
    init: () => {
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
};

// Form validation
const formValidation = {
    init: () => {
        const forms = document.querySelectorAll('.needs-validation');
        
        forms.forEach(form => {
            form.addEventListener('submit', (event) => {
                if (!form.checkValidity()) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                form.classList.add('was-validated');
            });
        });
    },

    validateEmail: (email) => {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    },

    validatePhone: (phone) => {
        const re = /^[0-9]{10,11}$/;
        return re.test(phone);
    }
};

// Animation utilities
const animations = {
    init: () => {
        const animationObserverOptions = {
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
        }, animationObserverOptions);
        
        // Observe elements for animation
        const animatedElements = document.querySelectorAll('.animate-on-scroll');
        animatedElements.forEach(el => observer.observe(el));
    },

    addParallax: () => {
        window.addEventListener('scroll', utils.debounce(() => {
            const scrolled = window.pageYOffset;
            const parallaxElements = document.querySelectorAll('.parallax');
            
            parallaxElements.forEach(element => {
                const speed = element.dataset.speed || 0.5;
                const yPos = -(scrolled * speed);
                element.style.transform = `translateY(${yPos}px)`;
            });
        }, 10));
    }
};

// Theme management
const theme = {
    init: () => {
        const savedTheme = localStorage.getItem('theme') || 'light';
        theme.setTheme(savedTheme);
        
        const themeToggle = document.querySelector('#themeToggle');
        if (themeToggle) {
            themeToggle.addEventListener('click', () => {
                const currentTheme = document.body.classList.contains('dark-theme') ? 'light' : 'dark';
                theme.setTheme(currentTheme);
            });
        }
    },

    setTheme: (themeName) => {
        document.body.classList.remove('light-theme', 'dark-theme');
        document.body.classList.add(`${themeName}-theme`);
        localStorage.setItem('theme', themeName);
    }
};

// Performance optimization
const performance = {
    init: () => {
        // Preload critical resources
        const criticalImages = [
            // Removed preloading of missing images
        ];
        
        criticalImages.forEach(src => {
            const link = document.createElement('link');
            link.rel = 'preload';
            link.as = 'image';
            link.href = src;
            document.head.appendChild(link);
        });
        
        // Service worker registration (if available)
        if ('serviceWorker' in navigator) {
            navigator.serviceWorker.register('/sw.js')
                .then(registration => console.log('SW registered'))
                .catch(error => console.log('SW registration failed'));
        }
    }
};

// Initialize all modules when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    // Initialize cart badge
    cart.updateCartBadge();
    
    // Initialize all modules
    search.init();
    productGallery.init();
    lazyLoading.init();
    formValidation.init();
    animations.init();
    animations.addParallax();
    theme.init();
    performance.init();
    
    // Add global event listeners
    addGlobalEventListeners();
    
    console.log('Linh Moc Store - JavaScript initialized');
});

// Global event listeners
function addGlobalEventListeners() {
    // Add to wishlist buttons
    document.addEventListener('click', (e) => {
        if (e.target.classList.contains('add-to-wishlist')) {
            e.preventDefault();
            const productId = e.target.dataset.productId;
            const productName = e.target.dataset.productName;
            const productPrice = parseFloat(e.target.dataset.productPrice);
            const productImage = e.target.dataset.productImage;
            
            wishlist.addItem({
                id: productId,
                name: productName,
                price: productPrice,
                image: productImage
            });
        }
    });
    
    // Smooth scrolling for anchor links
    document.addEventListener('click', (e) => {
        if (e.target.matches('a[href^="#"]')) {
            e.preventDefault();
            const target = document.querySelector(e.target.getAttribute('href'));
            if (target) {
                utils.scrollToElement(target, 100);
            }
        }
    });
}

// Export for use in other scripts
window.LinhMocStore = {
    utils,
    cart,
    wishlist,
    search,
    productGallery,
    formValidation,
    animations,
    theme
};

// Hàm đồng bộ giỏ hàng localStorage lên server sau khi đăng nhập
function syncCartToServer() {
    if (typeof isLoggedIn !== 'undefined' && isLoggedIn === 'true' && cartItems.length > 0) {
        fetch('/UserSite/Cart?handler=Sync', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify(cartItems)
        })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                localStorage.removeItem('cartItems');
                cartItems = [];
                cart.updateCartBadge();
            }
        });
    }
}

// Gọi hàm này sau khi đăng nhập thành công (có thể gọi ở trang trung gian hoặc sau khi load trang chủ)
