import { API_BASE_URL } from './config.js';
import { getAuthHeaders, showToast } from './utils.js';

export async function loadFriendRequests() {
    await Promise.all([loadReceivedRequests(), loadSentRequests()]);
}

async function loadReceivedRequests() {
    const container = document.getElementById("received-requests-list");
    if (!container) return;
    container.innerHTML = '<p class="empty-message">Yükleniyor...</p>';
    try {
        const response = await fetch(`${API_BASE_URL}/Friends/received`, { method: "GET", headers: getAuthHeaders() });
        if (!response.ok) throw new Error("Veri çekilemedi");
        const requests = await response.json();
        if (requests.length === 0) { container.innerHTML = '<p class="empty-message">Bekleyen arkadaşlık isteği yok.</p>'; return; }
        container.innerHTML = "";
        requests.forEach(req => {
            container.innerHTML += `
                <div class="request-item">
                    <div class="request-info"><h4>${req.friendName || 'İsimsiz'}</h4><span>${req.friendEmail}</span></div>
                    <div class="request-actions">
                        <button onclick="respondFriend(${req.requestId}, true)" class="btn-accept" title="Kabul Et"><span class="material-icons" style="font-size:18px;">check</span></button>
                        <button onclick="respondFriend(${req.requestId}, false)" class="btn-reject" title="Reddet"><span class="material-icons" style="font-size:18px;">close</span></button>
                    </div>
                </div>`;
        });
    } catch (error) { console.error(error); container.innerHTML = '<p class="empty-message" style="color:red">Hata oluştu.</p>'; }
}

async function loadSentRequests() {
    const container = document.getElementById("sent-requests-list");
    if (!container) return;
    container.innerHTML = '<p class="empty-message">Yükleniyor...</p>';
    try {
        const response = await fetch(`${API_BASE_URL}/Friends/sent`, { method: "GET", headers: getAuthHeaders() });
        if (!response.ok) throw new Error("Veri çekilemedi");
        const requests = await response.json();
        if (requests.length === 0) { container.innerHTML = '<p class="empty-message">Henüz kimseye istek göndermediniz.</p>'; return; }
        container.innerHTML = "";
        requests.forEach(req => {
            let statusClass = "status-pending", statusText = "Bekliyor", icon = "hourglass_empty";
            if (req.status === "Accepted") { statusClass = "status-accepted"; statusText = "Kabul Edildi"; icon = "check_circle"; }
            else if (req.status === "Rejected") { statusClass = "status-rejected"; statusText = "Reddedildi"; icon = "cancel"; }
            container.innerHTML += `
                <div class="request-item">
                    <div class="request-info"><h4>${req.friendName || 'Bilinmiyor'}</h4><span>${req.friendEmail}</span></div>
                    <div class="request-status"><span class="status-badge ${statusClass}"><span class="material-icons" style="font-size:12px;">${icon}</span> ${statusText}</span></div>
                </div>`;
        });
    } catch (error) { console.error(error); }
}

export async function loadActiveFriends() {
    const container = document.getElementById("active-friends-list");
    if (!container) return;
    container.innerHTML = '<p class="empty-message">Arkadaş listeniz yükleniyor...</p>';
    try {
        const response = await fetch(`${API_BASE_URL}/Friends/list`, { method: "GET", headers: getAuthHeaders() });
        if (!response.ok) throw new Error("Veri çekilemedi");
        const friends = await response.json();
        if (friends.length === 0) { container.innerHTML = '<p class="empty-message">Henüz hiç arkadaşınız yok.</p>'; return; }
        container.innerHTML = "";
        friends.forEach(friend => {
            container.innerHTML += `
                <div class="habit-item">
                    <div class="habit-info">
                        <div style="display:flex; align-items:center; gap:10px;">
                            <div style="width:40px; height:40px; background:#e0e7ff; border-radius:50%; display:flex; align-items:center; justify-content:center; color:#4F46E5;"><span class="material-icons">person</span></div>
                            <div><h4 style="margin:0;">${friend.friendName}</h4><span class="habit-tag" style="margin:0; font-size:12px;">${friend.friendEmail}</span></div>
                        </div>
                    </div>
                    <div class="habit-actions"><span class="status-badge status-accepted"><span class="material-icons" style="font-size:14px;">verified</span> Arkadaş</span></div>
                </div>`;
        });
    } catch (error) { console.error(error); container.innerHTML = '<p class="empty-message" style="color:red">Bağlantı hatası.</p>'; }
}

export function setupFriendSystem() {
    window.openFriendModal = function () { document.getElementById("friend-modal").classList.add("active"); };
    window.closeFriendModal = function () { document.getElementById("friend-modal").classList.remove("active"); };
    const friendForm = document.getElementById("send-friend-request-form");
    if (friendForm) {
        friendForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const emailInput = document.getElementById("friend-email");
            const btn = friendForm.querySelector("button");
            const originalText = btn.innerHTML;
            btn.innerHTML = "Gönderiliyor..."; btn.disabled = true;
            try {
                const response = await fetch(`${API_BASE_URL}/Friends/send-request`, {
                    method: "POST", headers: getAuthHeaders(),
                    body: JSON.stringify({ targetEmail: emailInput.value })
                });
                const data = await response.json();
                if (response.ok) { showToast("İstek gönderildi! 📩", "success"); emailInput.value = ""; window.closeFriendModal(); loadSentRequests(); }
                else { showToast(data.message || "İstek gönderilemedi.", "error"); }
            } catch (error) { console.error(error); showToast("Sunucu hatası.", "error"); }
            finally { btn.innerHTML = originalText; btn.disabled = false; }
        });
    }
}

window.respondFriend = async function (requestId, isAccepted) {
    try {
        const response = await fetch(`${API_BASE_URL}/Friends/respond`, {
            method: "POST", headers: getAuthHeaders(),
            body: JSON.stringify({ requestId: requestId, isAccepted: isAccepted })
        });
        const result = await response.json();
        if (response.ok) {
            showToast(isAccepted ? "Arkadaşlık kabul edildi! 🎉" : "İstek reddedildi.", "success");
            loadFriendRequests();
            loadActiveFriends();
        } else { showToast(result.message || "Bir hata oluştu.", "error"); }
    } catch (error) { console.error(error); showToast("Sunucu hatası.", "error"); }
};