import { API_BASE_URL } from './config.js';
import { getAuthHeaders, showToast } from './utils.js';

// ==========================================
// 1. ANA YÜKLEME FONKSİYONU
// ==========================================
export async function loadFriendRequests() {
    // Hem arkadaşlık isteklerini hem de alışkanlık davetlerini yükle
    await Promise.all([
        loadReceivedRequests(),
        loadSentRequests(),
        loadHabitInvitations() // <--- Yeni özellik
    ]);
}

// ==========================================
// 2. ARKADAŞLIK İSTEKLERİ (Gelen/Giden)
// ==========================================
async function loadReceivedRequests() {
    const container = document.getElementById("received-requests-list");
    if (!container) return;

    // Yükleniyor yazısı koymadan önce temizle
    container.innerHTML = '<p class="empty-message">Yükleniyor...</p>';

    try {
        const response = await fetch(`${API_BASE_URL}/Friends/received`, { method: "GET", headers: getAuthHeaders() });
        if (!response.ok) throw new Error("Veri çekilemedi");
        const requests = await response.json();

        // Alışkanlık davetleri için container'ı temizliyoruz ama başlıkları sonradan ekleyeceğiz
        container.innerHTML = "";

        // --- A. ARKADAŞLIK İSTEKLERİ ---
        if (requests.length > 0) {
            const title = document.createElement("h4");
            title.innerText = "Arkadaşlık İstekleri";
            title.style.cssText = "margin: 10px 0; color: #4F46E5;";
            container.appendChild(title);

            requests.forEach(req => {
                const div = document.createElement("div");
                div.className = "request-item";
                div.innerHTML = `
                    <div class="request-info">
                        <h4>${req.friendName || 'İsimsiz'}</h4>
                        <span>${req.friendEmail}</span>
                    </div>
                    <div class="request-actions">
                        <button onclick="respondFriend(${req.requestId}, true)" class="btn-accept" title="Kabul Et"><span class="material-icons" style="font-size:18px;">check</span></button>
                        <button onclick="respondFriend(${req.requestId}, false)" class="btn-reject" title="Reddet"><span class="material-icons" style="font-size:18px;">close</span></button>
                    </div>`;
                container.appendChild(div);
            });
        } else {
            // Eğer hiç arkadaşlık isteği yoksa (ve habit daveti de henüz yüklenmediyse)
            // Habit davetleri fonksiyonu buraya ekleme yapacak, şimdilik boş bırak.
        }

    } catch (error) {
        console.error(error);
        container.innerHTML = '<p class="empty-message" style="color:red">Hata oluştu.</p>';
    }
}

async function loadSentRequests() {
    const container = document.getElementById("sent-requests-list");
    if (!container) return;
    container.innerHTML = '<p class="empty-message">Yükleniyor...</p>';

    try {
        const response = await fetch(`${API_BASE_URL}/Friends/sent`, { method: "GET", headers: getAuthHeaders() });
        if (!response.ok) throw new Error("Veri çekilemedi");
        const requests = await response.json();

        if (requests.length === 0) {
            container.innerHTML = '<p class="empty-message">Henüz kimseye istek göndermediniz.</p>';
            return;
        }

        container.innerHTML = "";
        requests.forEach(req => {
            let statusClass = "status-pending";
            let statusText = "Bekliyor";
            let icon = "hourglass_empty";

            if (req.status === "Accepted") { statusClass = "status-accepted"; statusText = "Kabul Edildi"; icon = "check_circle"; }
            else if (req.status === "Rejected") { statusClass = "status-rejected"; statusText = "Reddedildi"; icon = "cancel"; }

            const html = `
                <div class="request-item">
                    <div class="request-info">
                        <h4>${req.friendName || 'Bilinmiyor'}</h4>
                        <span>${req.friendEmail}</span>
                    </div>
                    <div class="request-status">
                        <span class="status-badge ${statusClass}">
                            <span class="material-icons" style="font-size:12px;">${icon}</span>
                            ${statusText}
                        </span>
                    </div>
                </div>`;
            container.innerHTML += html;
        });
    } catch (error) { console.error(error); }
}

// ==========================================
// 3. ALIŞKANLIK DAVETLERİ (YENİ)
// ==========================================
async function loadHabitInvitations() {
    const container = document.getElementById("received-requests-list");
    if (!container) return;

    try {
        const response = await fetch(`${API_BASE_URL}/HabitShare/pending`, { method: "GET", headers: getAuthHeaders() });
        if (!response.ok) return; // Sessizce geç
        const invites = await response.json();

        if (invites.length > 0) {
            // Başlık ekle
            const title = document.createElement("h4");
            title.innerText = "Alışkanlık Davetleri";
            title.style.cssText = "margin: 20px 0 10px; color: #10B981;";
            container.appendChild(title);

            invites.forEach(inv => {
                const div = document.createElement("div");
                div.className = "request-item";
                div.style.background = "#f0fdf4"; // Yeşilimsi arka plan
                div.style.border = "1px solid #bbf7d0";
                div.innerHTML = `
                    <div class="request-info">
                        <h4 style="color:#15803d;">${inv.senderName}</h4>
                        <span style="font-size:12px;">Seni <strong>${inv.habitName}</strong> (${inv.category}) alışkanlığına davet ediyor!</span>
                    </div>
                    <div class="request-actions">
                        <button onclick="respondHabitShare(${inv.invitationId}, true)" class="btn-accept"><span class="material-icons">check</span></button>
                        <button onclick="respondHabitShare(${inv.invitationId}, false)" class="btn-reject"><span class="material-icons">close</span></button>
                    </div>`;
                container.appendChild(div);
            });
        }

        // Eğer hem arkadaşlık hem alışkanlık isteği yoksa boş mesaj göster
        if (container.children.length === 0) {
            container.innerHTML = '<p class="empty-message">Bekleyen istek yok.</p>';
        }

    } catch (e) { console.error(e); }
}

// ==========================================
// 4. AKTİF ARKADAŞLAR LİSTESİ
// ==========================================
export async function loadActiveFriends() {
    const container = document.getElementById("active-friends-list");
    if (!container) return;
    container.innerHTML = '<p class="empty-message">Arkadaş listeniz yükleniyor...</p>';

    try {
        const response = await fetch(`${API_BASE_URL}/Friends/list`, { method: "GET", headers: getAuthHeaders() });
        if (!response.ok) throw new Error("Veri çekilemedi");
        const friends = await response.json();

        if (friends.length === 0) {
            container.innerHTML = '<p class="empty-message">Henüz hiç arkadaşınız yok. İstekler menüsünden yeni arkadaşlar ekleyebilirsiniz!</p>';
            return;
        }

        container.innerHTML = "";
        friends.forEach(friend => {
            const html = `
                <div class="habit-item">
                    <div class="habit-info">
                        <div style="display:flex; align-items:center; gap:10px;">
                            <div style="width:40px; height:40px; background:#e0e7ff; border-radius:50%; display:flex; align-items:center; justify-content:center; color:#4F46E5;">
                                <span class="material-icons">person</span>
                            </div>
                            <div>
                                <h4 style="margin:0;">${friend.friendName}</h4>
                                <span class="habit-tag" style="margin:0; font-size:12px;">${friend.friendEmail}</span>
                            </div>
                        </div>
                    </div>
                    <div class="habit-actions">
                        <span class="status-badge status-accepted">
                            <span class="material-icons" style="font-size:14px;">verified</span> Arkadaş
                        </span>
                    </div>
                </div>`;
            container.innerHTML += html;
        });

    } catch (error) {
        console.error(error);
        container.innerHTML = '<p class="empty-message" style="color:red">Bağlantı hatası.</p>';
    }
}

// ==========================================
// 5. GLOBAL AKSİYONLAR (Window'a Atananlar)
// ==========================================

// Arkadaşlık İsteğine Cevap Ver
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

// Alışkanlık Davetine Cevap Ver
window.respondHabitShare = async function (id, accept) {
    try {
        const response = await fetch(`${API_BASE_URL}/HabitShare/respond`, {
            method: "POST", headers: getAuthHeaders(),
            body: JSON.stringify({ invitationId: id, isAccepted: accept })
        });
        if (response.ok) {
            showToast(accept ? "Alışkanlık eklendi! 🎯" : "Davet reddedildi.", "success");
            loadFriendRequests(); // Listeyi yenile
            // Alışkanlık listesini yenilemek için sayfayı yenileyebiliriz veya import edip çağırabiliriz
            // import { loadHabits } from './habits.js' burda çalışmayabilir, o yüzden en basiti:
            // Ana sayfaya geçince zaten yenileniyor.
        } else {
            showToast("İşlem başarısız.", "error");
        }
    } catch (e) { console.error(e); }
};

// --- PAYLAŞIM MODALI ---

window.openShareModal = async function (habitId) {
    document.getElementById("share-habit-id").value = habitId;
    const modal = document.getElementById("share-modal");
    const list = document.getElementById("share-friend-list");

    modal.classList.add("active");
    list.innerHTML = "Yükleniyor...";

    try {
        const response = await fetch(`${API_BASE_URL}/Friends/list`, { method: "GET", headers: getAuthHeaders() });
        const friends = await response.json();

        if (friends.length === 0) {
            list.innerHTML = "<p style='text-align:center; color:red;'>Henüz arkadaşınız yok.</p>";
            return;
        }

        list.innerHTML = "";
        friends.forEach(f => {
            // Not: FriendId veya RequestId, DTO'ya göre değişebilir. Genelde FriendId (User ID) olması gerekir.
            // Eğer çalışmazsa backend'den dönen property adını kontrol et.
            const friendId = f.friendId || f.requestId;

            list.innerHTML += `
                <label style="display:flex; align-items:center; gap:10px; padding:10px; border-bottom:1px solid #eee; cursor:pointer;">
                    <input type="radio" name="share_friend" value="${friendId}"> 
                    <div>
                        <div style="font-weight:bold;">${f.friendName}</div>
                        <div style="font-size:11px; color:#666;">${f.friendEmail}</div>
                    </div>
                </label>
            `;
        });

    } catch (e) { console.error(e); }
};

window.closeShareModal = function () {
    document.getElementById("share-modal").classList.remove("active");
};

window.submitShare = async function () {
    const habitId = document.getElementById("share-habit-id").value;
    const selected = document.querySelector('input[name="share_friend"]:checked');

    if (!selected) { showToast("Lütfen bir arkadaş seçin.", "error"); return; }

    try {
        const response = await fetch(`${API_BASE_URL}/HabitShare/send`, {
            method: "POST",
            headers: getAuthHeaders(),
            body: JSON.stringify({
                habitId: parseInt(habitId),
                friendId: parseInt(selected.value)
            })
        });

        if (response.ok) {
            showToast("Davet gönderildi!", "success");
            window.closeShareModal();
        } else {
            const err = await response.json();
            showToast(err.message || "Hata oluştu.", "error");
        }
    } catch (e) { console.error(e); showToast("Sunucu hatası.", "error"); }
};

// ==========================================
// 6. KURULUM (SETUP)
// ==========================================
export function setupFriendSystem() {
    // Arkadaş Ekle Modalı
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
                if (response.ok) {
                    showToast("İstek gönderildi! 📩", "success");
                    emailInput.value = "";
                    window.closeFriendModal();
                    loadSentRequests();
                } else { showToast(data.message || "İstek gönderilemedi.", "error"); }
            } catch (error) { console.error(error); showToast("Sunucu hatası.", "error"); }
            finally { btn.innerHTML = originalText; btn.disabled = false; }
        });
    }
}