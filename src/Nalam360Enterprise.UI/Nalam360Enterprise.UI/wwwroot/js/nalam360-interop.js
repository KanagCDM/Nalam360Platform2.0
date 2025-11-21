/**
 * Nalam360 Enterprise UI - JavaScript Interop Module
 * Centralized browser API interactions for Blazor components
 * Version: 1.0.0
 */

export const Nalam360Interop = {
    /**
     * Clipboard Operations
     */
    clipboard: {
        async copyToClipboard(text) {
            try {
                await navigator.clipboard.writeText(text);
                return { success: true, message: 'Copied to clipboard' };
            } catch (error) {
                console.error('Clipboard copy failed:', error);
                return { success: false, message: error.message };
            }
        },

        async readFromClipboard() {
            try {
                const text = await navigator.clipboard.readText();
                return { success: true, data: text };
            } catch (error) {
                console.error('Clipboard read failed:', error);
                return { success: false, message: error.message };
            }
        }
    },

    /**
     * Local Storage Operations
     */
    localStorage: {
        getItem(key) {
            try {
                return window.localStorage.getItem(key);
            } catch (error) {
                console.error('LocalStorage get failed:', error);
                return null;
            }
        },

        setItem(key, value) {
            try {
                window.localStorage.setItem(key, value);
                return { success: true };
            } catch (error) {
                console.error('LocalStorage set failed:', error);
                return { success: false, message: error.message };
            }
        },

        removeItem(key) {
            try {
                window.localStorage.removeItem(key);
                return { success: true };
            } catch (error) {
                console.error('LocalStorage remove failed:', error);
                return { success: false, message: error.message };
            }
        },

        clear() {
            try {
                window.localStorage.clear();
                return { success: true };
            } catch (error) {
                console.error('LocalStorage clear failed:', error);
                return { success: false, message: error.message };
            }
        }
    },

    /**
     * Session Storage Operations
     */
    sessionStorage: {
        getItem(key) {
            try {
                return window.sessionStorage.getItem(key);
            } catch (error) {
                console.error('SessionStorage get failed:', error);
                return null;
            }
        },

        setItem(key, value) {
            try {
                window.sessionStorage.setItem(key, value);
                return { success: true };
            } catch (error) {
                console.error('SessionStorage set failed:', error);
                return { success: false, message: error.message };
            }
        },

        removeItem(key) {
            try {
                window.sessionStorage.removeItem(key);
                return { success: true };
            } catch (error) {
                console.error('SessionStorage remove failed:', error);
                return { success: false, message: error.message };
            }
        },

        clear() {
            try {
                window.sessionStorage.clear();
                return { success: true };
            } catch (error) {
                console.error('SessionStorage clear failed:', error);
                return { success: false, message: error.message };
            }
        }
    },

    /**
     * Focus Management
     */
    focus: {
        focusElement(selector) {
            try {
                const element = document.querySelector(selector);
                if (element) {
                    element.focus();
                    return { success: true };
                }
                return { success: false, message: 'Element not found' };
            } catch (error) {
                console.error('Focus failed:', error);
                return { success: false, message: error.message };
            }
        },

        focusFirstInvalidField() {
            try {
                const invalidField = document.querySelector('.is-invalid, [aria-invalid="true"]');
                if (invalidField) {
                    invalidField.focus();
                    return { success: true };
                }
                return { success: false, message: 'No invalid field found' };
            } catch (error) {
                console.error('Focus invalid field failed:', error);
                return { success: false, message: error.message };
            }
        },

        blurElement(selector) {
            try {
                const element = document.querySelector(selector);
                if (element) {
                    element.blur();
                    return { success: true };
                }
                return { success: false, message: 'Element not found' };
            } catch (error) {
                console.error('Blur failed:', error);
                return { success: false, message: error.message };
            }
        }
    },

    /**
     * Scroll Operations
     */
    scroll: {
        scrollToElement(selector, behavior = 'smooth') {
            try {
                const element = document.querySelector(selector);
                if (element) {
                    element.scrollIntoView({ behavior, block: 'start' });
                    return { success: true };
                }
                return { success: false, message: 'Element not found' };
            } catch (error) {
                console.error('Scroll to element failed:', error);
                return { success: false, message: error.message };
            }
        },

        scrollToTop(behavior = 'smooth') {
            try {
                window.scrollTo({ top: 0, behavior });
                return { success: true };
            } catch (error) {
                console.error('Scroll to top failed:', error);
                return { success: false, message: error.message };
            }
        },

        scrollToBottom(behavior = 'smooth') {
            try {
                window.scrollTo({ top: document.body.scrollHeight, behavior });
                return { success: true };
            } catch (error) {
                console.error('Scroll to bottom failed:', error);
                return { success: false, message: error.message };
            }
        },

        getScrollPosition() {
            try {
                return {
                    success: true,
                    x: window.pageXOffset || document.documentElement.scrollLeft,
                    y: window.pageYOffset || document.documentElement.scrollTop
                };
            } catch (error) {
                console.error('Get scroll position failed:', error);
                return { success: false, message: error.message };
            }
        }
    },

    /**
     * File Download
     */
    file: {
        downloadFile(fileName, content, mimeType = 'application/octet-stream') {
            try {
                const blob = new Blob([content], { type: mimeType });
                const url = URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = fileName;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                URL.revokeObjectURL(url);
                return { success: true };
            } catch (error) {
                console.error('File download failed:', error);
                return { success: false, message: error.message };
            }
        },

        downloadBase64(fileName, base64Content, mimeType = 'application/octet-stream') {
            try {
                const byteCharacters = atob(base64Content);
                const byteNumbers = new Array(byteCharacters.length);
                for (let i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                const byteArray = new Uint8Array(byteNumbers);
                const blob = new Blob([byteArray], { type: mimeType });
                const url = URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = url;
                link.download = fileName;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                URL.revokeObjectURL(url);
                return { success: true };
            } catch (error) {
                console.error('Base64 download failed:', error);
                return { success: false, message: error.message };
            }
        }
    },

    /**
     * Print Operations
     */
    print: {
        printElement(selector) {
            try {
                const element = document.querySelector(selector);
                if (!element) {
                    return { success: false, message: 'Element not found' };
                }

                const printWindow = window.open('', '_blank');
                printWindow.document.write('<html><head><title>Print</title>');
                printWindow.document.write('<style>body { margin: 0; padding: 20px; }</style>');
                
                // Copy stylesheets
                const stylesheets = document.querySelectorAll('link[rel="stylesheet"], style');
                stylesheets.forEach(sheet => {
                    printWindow.document.write(sheet.outerHTML);
                });
                
                printWindow.document.write('</head><body>');
                printWindow.document.write(element.innerHTML);
                printWindow.document.write('</body></html>');
                printWindow.document.close();
                
                printWindow.onload = () => {
                    printWindow.print();
                    printWindow.close();
                };
                
                return { success: true };
            } catch (error) {
                console.error('Print element failed:', error);
                return { success: false, message: error.message };
            }
        },

        printPage() {
            try {
                window.print();
                return { success: true };
            } catch (error) {
                console.error('Print page failed:', error);
                return { success: false, message: error.message };
            }
        }
    },

    /**
     * Browser Information
     */
    browser: {
        getBrowserInfo() {
            try {
                const userAgent = navigator.userAgent;
                const isChrome = /Chrome/.test(userAgent) && /Google Inc/.test(navigator.vendor);
                const isFirefox = /Firefox/.test(userAgent);
                const isSafari = /Safari/.test(userAgent) && /Apple Computer/.test(navigator.vendor);
                const isEdge = /Edg/.test(userAgent);
                const isOpera = /OPR/.test(userAgent);

                return {
                    success: true,
                    userAgent,
                    isChrome,
                    isFirefox,
                    isSafari,
                    isEdge,
                    isOpera,
                    isMobile: /Mobile|Android|iPhone|iPad/.test(userAgent),
                    isTouch: 'ontouchstart' in window,
                    language: navigator.language,
                    platform: navigator.platform
                };
            } catch (error) {
                console.error('Get browser info failed:', error);
                return { success: false, message: error.message };
            }
        },

        getViewportSize() {
            try {
                return {
                    success: true,
                    width: window.innerWidth,
                    height: window.innerHeight
                };
            } catch (error) {
                console.error('Get viewport size failed:', error);
                return { success: false, message: error.message };
            }
        }
    },

    /**
     * Element Operations
     */
    element: {
        addClass(selector, className) {
            try {
                const element = document.querySelector(selector);
                if (element) {
                    element.classList.add(className);
                    return { success: true };
                }
                return { success: false, message: 'Element not found' };
            } catch (error) {
                console.error('Add class failed:', error);
                return { success: false, message: error.message };
            }
        },

        removeClass(selector, className) {
            try {
                const element = document.querySelector(selector);
                if (element) {
                    element.classList.remove(className);
                    return { success: true };
                }
                return { success: false, message: 'Element not found' };
            } catch (error) {
                console.error('Remove class failed:', error);
                return { success: false, message: error.message };
            }
        },

        toggleClass(selector, className) {
            try {
                const element = document.querySelector(selector);
                if (element) {
                    element.classList.toggle(className);
                    return { success: true, hasClass: element.classList.contains(className) };
                }
                return { success: false, message: 'Element not found' };
            } catch (error) {
                console.error('Toggle class failed:', error);
                return { success: false, message: error.message };
            }
        },

        getBoundingRect(selector) {
            try {
                const element = document.querySelector(selector);
                if (element) {
                    const rect = element.getBoundingClientRect();
                    return {
                        success: true,
                        top: rect.top,
                        right: rect.right,
                        bottom: rect.bottom,
                        left: rect.left,
                        width: rect.width,
                        height: rect.height,
                        x: rect.x,
                        y: rect.y
                    };
                }
                return { success: false, message: 'Element not found' };
            } catch (error) {
                console.error('Get bounding rect failed:', error);
                return { success: false, message: error.message };
            }
        }
    },

    /**
     * Event Listeners
     */
    events: {
        addEventListener(selector, eventName, dotNetHelper, methodName) {
            try {
                const element = document.querySelector(selector);
                if (!element) {
                    return { success: false, message: 'Element not found' };
                }

                const handler = (event) => {
                    dotNetHelper.invokeMethodAsync(methodName, {
                        type: event.type,
                        target: event.target.id || event.target.className
                    });
                };

                element.addEventListener(eventName, handler);
                
                // Store handler for cleanup
                if (!window._nalam360Handlers) {
                    window._nalam360Handlers = new Map();
                }
                const key = `${selector}_${eventName}`;
                window._nalam360Handlers.set(key, handler);

                return { success: true };
            } catch (error) {
                console.error('Add event listener failed:', error);
                return { success: false, message: error.message };
            }
        },

        removeEventListener(selector, eventName) {
            try {
                const element = document.querySelector(selector);
                if (!element) {
                    return { success: false, message: 'Element not found' };
                }

                const key = `${selector}_${eventName}`;
                const handler = window._nalam360Handlers?.get(key);
                
                if (handler) {
                    element.removeEventListener(eventName, handler);
                    window._nalam360Handlers.delete(key);
                    return { success: true };
                }

                return { success: false, message: 'Handler not found' };
            } catch (error) {
                console.error('Remove event listener failed:', error);
                return { success: false, message: error.message };
            }
        }
    }
};

// Legacy compatibility - expose on window for direct calls
window.Nalam360Interop = Nalam360Interop;
