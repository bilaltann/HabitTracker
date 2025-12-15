import { showToast } from './utils.js';

const API_BASE_URL = "https://localhost:7223/api";


function getHeaders() {
    const token = localStorage.getItem("jwtToken");
    return {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
    };
}


window.switchTab = function (tabName, element) {
    // Menüdeki aktifliði deðiþtir
    document.querySelectorAll('.nav-item').forEach(el => el.classList.remove('active'));
    element.classList.add('active');

    // Ýçerik alanýný deðiþtir
    document.querySelectorAll('.tab-content').forEach(el => el.classList.remove('active'));
    document.getElementById('tab-' + tabName).classList.add('active');

    // Verileri sadece ihtiyaç olunca yükle
    if (tabName === 'dashboard') loadStats();
    if (tabName === 'users') loadUsers();
    if (tabName === 'friendships') loadFriendships();
}

// DASHBOARD ÝSTATÝSTÝKLERÝ 
window.loadStats = async function () {
    // Mevcut switchTab fonksiyonunu güncelle:
    if (tabName === 'dashboard') {
        loadStats();
        loadActivities();     // <-- EKLE
        loadSystemHealth();   // <-- EKLE
    }

    // Mevcut DOMContentLoaded kýsmýný güncelle:
    document.addEventListener('DOMContentLoaded', () => {
        loadStats();
        loadActivities();     // <-- EKLE
        loadSystemHealth();   // <-- EKLE
    });
    try {
        const response = await fetch(`${API_BASE_URL}/Admin/stats`, { headers: getHeaders() });
        if (!response.ok) throw new Error("Veri alýnamadý");

        const data = await response.json();

        // Verileri ekrana bas
        document.getElementById('stat-total-users').textContent = data.totalUsers || 0;
        document.getElementById('stat-total-friendships').textContent = data.totalFriendships || 0;
        document.getElementById('stat-total-habits').textContent = data.totalHabits || 0;

    } catch (error) {
        console.warn("Ýstatistikler yüklenemedi: Backend kapalý olabilir.");
    }
}

//KULLANICI LÝSTESÝ
window.loadUsers = async function () {
    const container = document.getElementById('user-list-container');
    container.innerHTML = '<tr><td colspan="5" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const response = await fetch(`${API_BASE_URL}/Admin/users`, { headers: getHeaders() });
        if (!response.ok) throw new Error("Hata oluþtu");

        const users = await response.json();
        container.innerHTML = "";

        if (users.length === 0) {
            container.innerHTML = '<tr><td colspan="5" style="text-align:center;">Kayýt bulunamadý.</td></tr>';
            return;
        }

        users.forEach(user => {
            const badgeClass = user.isActive ? 'badge-success' : 'badge-danger';
            const statusText = user.isActive ? 'Aktif' : 'Pasif';
            const userRole = user.role || 'User';
            const userName = user.name || 'Ýsimsiz';

            const row = `
                <tr>
                    <td>
                        <div style="display:flex; align-items:center;">
                            <div class="avatar-circle">${userName.charAt(0).toUpperCase()}</div>
                            <strong>${userName}</strong>
                        </div>
                    </td>
                    <td>${user.email}</td>
                    <td>${userRole}</td>
                    <td><span class="badge ${badgeClass}">${statusText}</span></td>
                    <td style="text-align:right;">
                        <button class="btn-icon btn-edit" onclick="editUser(${user.id})"><span class="material-icons" style="font-size:18px;">edit</span></button>
                        <button class="btn-icon btn-delete" onclick="deleteUser(${user.id})"><span class="material-icons" style="font-size:18px;">delete</span></button>
                    </td>
                </tr>
            `;
            container.innerHTML += row;
        });

    } catch (error) {
        container.innerHTML = `<tr><td colspan="5" style="text-align:center; color:red;">Baðlantý Hatasý: ${error.message}</td></tr>`;
        showToast("Kullanýcý listesi alýnamadý", "error");
    }
}

//ARKADAÞLIK LÝSTESÝ
window.loadFriendships = async function () {
    const container = document.getElementById('friendship-list-container');
    container.innerHTML = '<div style="text-align:center;">Yükleniyor...</div>';

    try {
        const response = await fetch(`${API_BASE_URL}/Admin/friendships`, { headers: getHeaders() });
        if (!response.ok) throw new Error("Hata oluþtu");

        const friendships = await response.json();
        container.innerHTML = "";

        if (friendships.length === 0) {
            container.innerHTML = '<div style="text-align:center; color:#666;">Kayýt yok.</div>';
            return;
        }

        friendships.forEach(f => {
            let badgeClass = 'badge-warning';
            let iconColor = '#f59e0b';

            if (f.status === 'Accepted') { badgeClass = 'badge-success'; iconColor = '#10b981'; }
            if (f.status === 'Rejected') { badgeClass = 'badge-danger'; iconColor = '#ef4444'; }

            const card = `
                <div class="friendship-card">
                    <div>
                        <div style="display:flex; align-items:center; gap:10px; font-weight:600; color:#333;">
                            <span>${f.user1Name}</span>
                            <span class="material-icons" style="color:${iconColor}; font-size:18px;">arrow_forward</span>
                            <span>${f.user2Name}</span>
                        </div>
                        <div style="font-size:12px; color:#999; margin-top:5px;">ID: ${f.id} • ${new Date(f.createdAt).toLocaleDateString()}</div>
                    </div>
                    <div style="text-align:right; display:flex; flex-direction:column; gap:5px; align-items:flex-end;">
                        <span class="badge ${badgeClass}">${f.status}</span>
                        <button class="btn-icon btn-delete" onclick="deleteFriendship(${f.id})" style="width:28px; height:28px;">
                            <span class="material-icons" style="font-size:16px;">close</span>
                        </button>
                    </div>
                </div>
            `;
            container.innerHTML += card;
        });

    } catch (error) {
        container.innerHTML = `<div style="text-align:center; color:red;">Hata: ${error.message}</div>`;
    }
}

//SÝLME / DÜZENLEME
window.deleteUser = async function (id) {
    if (!confirm("Bu kullanýcý silinecek. Emin misiniz?")) return;

    try {
        const res = await fetch(`${API_BASE_URL}/Admin/users/${id}`, {
            method: 'DELETE', headers: getHeaders()
        });

        if (res.ok) {
            showToast("Kullanýcý silindi", "success");
            loadUsers();
        } else {
            showToast("Silinemedi", "error");
        }
    } catch (e) { showToast("Sunucu hatasý", "error"); }
}

window.deleteFriendship = async function (id) {
    if (!confirm("Arkadaþlýk silinecek. Onaylýyor musunuz?")) return;

    try {
        const res = await fetch(`${API_BASE_URL}/Admin/friendships/${id}`, {
            method: 'DELETE', headers: getHeaders()
        });

        if (res.ok) {
            showToast("Arkadaþlýk silindi", "success");
            loadFriendships();
        } else {
            showToast("Hata oluþtu", "error");
        }
    } catch (e) { showToast("Sunucu hatasý", "error"); }
}

window.editUser = function (id) {
    alert("Düzenleme modalý backend entegrasyonundan sonra eklenebilir. ID: " + id);
}

//BAÞLANGIÇ
document.addEventListener('DOMContentLoaded', () => {
    loadStats(); 
});

// Çýkýþ Butonu
const logoutBtn = document.getElementById('logout-btn');
if (logoutBtn) {
    logoutBtn.addEventListener('click', () => {
        localStorage.removeItem('jwtToken');
        window.location.href = 'admin_login.html';
    });
}




window.loadActivities = async function () {
    const container = document.getElementById('activity-list');

    try {
        
        const response = await fetch(`${API_BASE_URL}/Admin/activities`, { headers: getHeaders() });

        if (!response.ok) throw new Error("Veri çekilemedi");
        const activities = await response.json();

        container.innerHTML = ""; 

        if (activities.length === 0) {
            container.innerHTML = '<div class="empty-state"><p>Henüz aktivite kaydý yok.</p></div>';
            return;
        }

        activities.forEach(act => {
            
            let colorClass = 'bg-blue';
            let iconName = 'info';

            if (act.type === 'success') { colorClass = 'bg-green'; iconName = 'check_circle'; }
            if (act.type === 'warning') { colorClass = 'bg-orange'; iconName = 'warning'; }

            const html = `
                <div class="activity-item">
                    <div class="act-icon ${colorClass}">
                        <span class="material-icons">${iconName}</span>
                    </div>
                    <div class="act-details">
                        <h4>${act.title}</h4>
                        <p>${act.description}</p>
                    </div>
                    <span class="act-time">${new Date(act.date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                </div>
            `;
            container.innerHTML += html;
        });

    } catch (error) {
        
        container.innerHTML = `
            <div class="empty-state">
                <span class="material-icons" style="color:#ccc;">cloud_off</span>
                <p>Verilere ulaþýlamadý.</p>
                <small style="color:#ccc; font-size:11px;">Backend baðlantýsýný kontrol edin.</small>
            </div>`;
    }
}


window.loadSystemHealth = async function () {
    try {
        
        const response = await fetch(`${API_BASE_URL}/Admin/system-health`, { headers: getHeaders() });

        if (response.ok) {
            const data = await response.json();

            // Veri gelirse çubuklarý güncelle
            document.getElementById('cpu-val').textContent = data.cpu + '%';
            document.getElementById('cpu-bar').style.width = data.cpu + '%';

            document.getElementById('ram-val').textContent = data.ram + '%';
            document.getElementById('ram-bar').style.width = data.ram + '%';

            const dbLabel = document.getElementById('db-status');
            if (data.dbStatus) {
                dbLabel.textContent = "Baðlý (Online)";
                dbLabel.className = "status-badge status-ok";
            } else {
                dbLabel.textContent = "Baðlantý Hatasý";
                dbLabel.className = "status-badge status-err";
            }
        }
    } catch (e) {
        
        console.warn("Sistem durumu çekilemedi.");
        document.getElementById('db-status').textContent = "Bilinmiyor";
        document.getElementById('db-status').className = "status-badge status-err";
    }
}