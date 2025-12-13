export function checkAuth() {
    if (!localStorage.getItem("jwtToken")) window.location.href = "login.html";
}
export function getAuthHeaders() {
    return { "Content-Type": "application/json", "Authorization": `Bearer ${localStorage.getItem("jwtToken")}` };
}
export function showToast(message, type = 'success') {
    let container = document.getElementById('toast-container');
    if (!container) { container = document.createElement('div'); container.id = 'toast-container'; document.body.appendChild(container); }
    const iconName = type === 'success' ? 'check_circle' : 'error';
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    toast.innerHTML = `<div class="toast-content"><span class="material-icons toast-icon">${iconName}</span><span class="toast-message">${message}</span></div>`;
    toast.onclick = function () { this.remove(); };
    container.appendChild(toast);
    setTimeout(() => { toast.remove(); }, 3000);
}
export function parseJwt(token) {
    try {
        var base64Url = token.split('.')[1];
        var base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        var jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function (c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (e) { return null; }
}