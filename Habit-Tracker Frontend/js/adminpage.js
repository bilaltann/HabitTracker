import { showToast } from './utils.js';

const API_BASE_URL = "http://3.88.214.139:8080/api";

// Tüm kullanıcıları hafızada tutmak için global değişken
let allUsersData = [];

function getHeaders() {
    const token = localStorage.getItem("jwtToken");
    return {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`
    };
}

// ==========================================
// 1. SAYFA GEÇİŞLERİ VE BAŞLANGIÇ
// ==========================================
window.switchTab = function (tabName, element) {
    document.querySelectorAll('.nav-item').forEach(el => el.classList.remove('active'));
    element.classList.add('active');

    document.querySelectorAll('.tab-content').forEach(el => el.classList.remove('active'));
    const targetTab = document.getElementById('tab-' + tabName);
    if (targetTab) targetTab.classList.add('active');

    if (tabName === 'dashboard') {
        loadStats();
        loadActivities();
        loadSystemHealth();
    }
    if (tabName === 'users') loadUsers();
    if (tabName === 'friendships') loadFriendships();
    if (tabName === 'logs') loadSystemLogs(); // <-- EKLENDİ
    if (tabName === 'badges') loadBadges();

};

document.addEventListener('DOMContentLoaded', () => {
    loadStats();
    loadActivities();
    loadSystemHealth();

    // Çıkış Butonu
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', () => {
            localStorage.removeItem('jwtToken');
            window.location.href = 'admin_login.html';
        });
    }

    // Düzenleme Formu
    const editForm = document.getElementById('edit-user-form');
    if (editForm) {
        editForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const id = document.getElementById('edit-user-id').value;
            const name = document.getElementById('edit-user-name').value;
            const email = document.getElementById('edit-user-email').value;
            const role = document.getElementById('edit-user-role').value;
            const btn = editForm.querySelector('button[type="submit"]');
            const originalText = btn.textContent;

            btn.textContent = "Güncelleniyor...";
            btn.disabled = true;

            try {
                const response = await fetch(`${API_BASE_URL}/Admin/users/${id}`, {
                    method: 'PUT',
                    headers: getHeaders(),
                    body: JSON.stringify({ id: parseInt(id), name, email, role })
                });

                if (response.ok) {
                    showToast("Kullanıcı güncellendi", "success");
                    closeEditUserModal();
                    loadUsers();
                } else {
                    const err = await response.json();
                    showToast(err.message || "Güncelleme başarısız", "error");
                }
            } catch (error) {
                console.error(error);
                showToast("Sunucu hatası", "error");
            } finally {
                btn.textContent = originalText;
                btn.disabled = false;
            }
        });
    }

    // --- ARAMA KUTUSU DİNLEYİCİSİ (YENİ EKLENDİ) ---
    const searchInput = document.getElementById('user-search-input');
    if (searchInput) {
        searchInput.addEventListener('input', (e) => {
            const searchTerm = e.target.value.toLowerCase();

            // Global veriden filtrele
            const filteredUsers = allUsersData.filter(user => {
                const name = (user.name || "").toLowerCase();
                const email = (user.email || "").toLowerCase();
                return name.includes(searchTerm) || email.includes(searchTerm);
            });

            // Filtrelenmiş veriyi çiz
            renderUsersTable(filteredUsers);
        });
    }
});

// ==========================================
// 2. KULLANICI LİSTESİ VE FİLTRELEME
// ==========================================

// Backend'den veriyi çeken fonksiyon
window.loadUsers = async function () {
    const container = document.getElementById('user-list-container');
    if (!container) return;

    container.innerHTML = '<tr><td colspan="4" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const response = await fetch(`${API_BASE_URL}/Admin/users`, { headers: getHeaders() });
        if (!response.ok) throw new Error("Hata oluştu");

        // Veriyi global değişkene at
        allUsersData = await response.json();

        // Tabloyu çiz
        renderUsersTable(allUsersData);

    } catch (error) {
        container.innerHTML = `<tr><td colspan="4" style="text-align:center; color:red;">Bağlantı Hatası: ${error.message}</td></tr>`;
        showToast("Kullanıcı listesi alınamadı", "error");
    }
};

// Tabloyu HTML'e basan fonksiyon (Arama yapınca burası çağrılır)
function renderUsersTable(users) {
    const container = document.getElementById('user-list-container');
    container.innerHTML = "";

    if (users.length === 0) {
        container.innerHTML = '<tr><td colspan="4" style="text-align:center; color:#999;">Kayıt bulunamadı.</td></tr>';
        return;
    }

    users.forEach(user => {
        const userRole = user.role ? user.role.toString() : 'User';
        const userName = user.name || 'İsimsiz';

        let badgeClass = 'badge-success';
        if (userRole.toLowerCase() === 'admin') {
            badgeClass = 'badge-danger';
        }

        const row = `
            <tr>
                <td>
                    <div style="display:flex; align-items:center;">
                        <div class="avatar-circle">${userName.charAt(0).toUpperCase()}</div>
                        <strong>${userName}</strong>
                    </div>
                </td>
                <td>${user.email}</td>
                <td><span class="badge ${badgeClass}">${userRole}</span></td>
                <td style="text-align:right;">
                    <button class="btn-icon btn-edit" onclick="openEditUserModal(${user.id}, '${userName}', '${user.email}', '${userRole}')">
                        <span class="material-icons" style="font-size:18px;">edit</span>
                    </button>
                    <button class="btn-icon btn-delete" onclick="openDeleteUserModal(${user.id})">
                        <span class="material-icons" style="font-size:18px;">delete</span>
                    </button>
                </td>
            </tr>`;
        container.innerHTML += row;
    });
}

// ==========================================
// 3. ARKADAŞLIK LİSTESİ
// ==========================================
window.loadFriendships = async function () {
    const container = document.getElementById('friendship-list-container');
    if (!container) return;

    container.innerHTML = '<div style="text-align:center;">Yükleniyor...</div>';

    try {
        const response = await fetch(`${API_BASE_URL}/Admin/friendships`, { headers: getHeaders() });
        if (!response.ok) throw new Error("Hata oluştu");

        const friendships = await response.json();
        container.innerHTML = "";

        if (friendships.length === 0) {
            container.innerHTML = '<div style="text-align:center; color:#666;">Kayıt yok.</div>';
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
                            <span>${f.requesterName}</span>
                            <span class="material-icons" style="color:${iconColor}; font-size:18px;">arrow_forward</span>
                            <span>${f.addresseeName}</span>
                        </div>
                        <div style="font-size:12px; color:#999; margin-top:5px;">ID: ${f.id} • ${new Date(f.createdDate).toLocaleDateString()}</div>
                    </div>
                    <div style="text-align:right; display:flex; flex-direction:column; gap:5px; align-items:flex-end;">
                        <span class="badge ${badgeClass}">${f.status}</span>
                        <button class="btn-icon btn-delete" onclick="openDeleteFriendshipModal(${f.id})" style="width:28px; height:28px;">
                            <span class="material-icons" style="font-size:16px;">close</span>
                        </button>
                    </div>
                </div>`;
            container.innerHTML += card;
        });

    } catch (error) {
        container.innerHTML = `<div style="text-align:center; color:red;">Hata: ${error.message}</div>`;
    }
};

// ==========================================
// 4. DASHBOARD İSTATİSTİKLERİ
// ==========================================
window.loadStats = async function () {
    try {
        // 1. Sadece Kullanıcı Listesi Endpoint'ine gidiyoruz
        const response = await fetch(`${API_BASE_URL}/Admin/users`, { headers: getHeaders() });
        const response2 = await fetch(`${API_BASE_URL}/Admin/habits`, { headers: getHeaders() });

        if (!response.ok || !response2.ok) throw new Error("Veri alınamadı");

        // 2. Backend bir LİSTE ([...]) dönüyor
        const usersList = await response.json();
        const habitsList = await response2.json();
        // 3. HTML elemanını seç
        const elUser = document.getElementById('stat-total-users');
        const elHabit = document.getElementById('stat-total-habits');


        // 4. Listenin uzunluğunu (.length) yazdır
        if (elUser) {
            elUser.textContent = usersList.length; // Toplam kullanıcı sayısı
        }

        if (elHabit) {
            elHabit.textContent = habitsList.length; // Toplam akışkanlık sayısı
        }


    } catch (error) {
        console.warn("İstatistikler yüklenemedi:", error);
    }
};
window.loadActivities = async function () {
    const container = document.getElementById('activity-list');
    if (!container) return;

    try {
        const response = await fetch(`${API_BASE_URL}/Admin/activities`, { headers: getHeaders() });
        if (!response.ok) throw new Error("Veri çekilemedi");
        const activities = await response.json();

        container.innerHTML = "";
        if (activities.length === 0) {
            container.innerHTML = '<div class="empty-state"><p>Henüz aktivite kaydı yok.</p></div>';
            return;
        }

        activities.forEach(act => {
            let colorClass = 'bg-blue';
            let iconName = 'info';
            if (act.type === 'success') { colorClass = 'bg-green'; iconName = 'check_circle'; }
            if (act.type === 'warning') { colorClass = 'bg-orange'; iconName = 'warning'; }

            container.innerHTML += `
                <div class="activity-item">
                    <div class="act-icon ${colorClass}"><span class="material-icons">${iconName}</span></div>
                    <div class="act-details"><h4>${act.title}</h4><p>${act.description}</p></div>
                    <span class="act-time">${new Date(act.date).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                </div>`;
        });
    } catch (error) {
        container.innerHTML = `<div class="empty-state"><p>Verilere ulaşılamadı.</p></div>`;
    }
};

window.loadSystemHealth = async function () {
    try {
        const response = await fetch(`${API_BASE_URL}/Admin/system-health`, { headers: getHeaders() });
        if (response.ok) {
            const data = await response.json();

            const cpuEl = document.getElementById('cpu-val');
            const ramEl = document.getElementById('ram-val');
            const cpuBar = document.getElementById('cpu-bar');
            const ramBar = document.getElementById('ram-bar');
            const dbLabel = document.getElementById('db-status');

            if (cpuEl) cpuEl.textContent = data.cpu + '%';
            if (cpuBar) cpuBar.style.width = data.cpu + '%';
            if (ramEl) ramEl.textContent = data.ram + '%';
            if (ramBar) ramBar.style.width = data.ram + '%';

            if (dbLabel) {
                if (data.dbStatus) {
                    dbLabel.textContent = "Bağlı (Online)";
                    dbLabel.className = "status-badge status-ok";
                } else {
                    dbLabel.textContent = "Bağlantı Hatası";
                    dbLabel.className = "status-badge status-err";
                }
            }
        }
    } catch (e) { console.warn("Sistem durumu çekilemedi."); }
};


// ==========================================
// 5. MODAL VE FORM İŞLEMLERİ
// ==========================================

// -- KULLANICI DÜZENLEME --
window.openEditUserModal = function (id, name, email, role) {
    document.getElementById('edit-user-id').value = id;
    document.getElementById('edit-user-name').value = name;
    document.getElementById('edit-user-email').value = email;
    document.getElementById('edit-user-role').value = role;
    document.getElementById('edit-user-modal').classList.add('active');
};

window.closeEditUserModal = function () {
    document.getElementById('edit-user-modal').classList.remove('active');
};

// -- KULLANICI SİLME --
window.openDeleteUserModal = function (id) {
    document.getElementById('delete-user-id-input').value = id;
    document.getElementById('delete-user-modal').classList.add('active');
};

window.closeDeleteUserModal = function () {
    document.getElementById('delete-user-modal').classList.remove('active');
};

window.confirmDeleteUser = async function () {
    const id = document.getElementById('delete-user-id-input').value;
    const btn = document.querySelector('#delete-user-modal .btn-delete-confirm');
    const originalText = btn.innerText;

    btn.innerText = "Siliniyor...";
    btn.disabled = true;

    try {
        const res = await fetch(`${API_BASE_URL}/Admin/users/${id}`, {
            method: 'DELETE', headers: getHeaders()
        });

        if (res.ok) {
            showToast("Kullanıcı silindi", "success");
            closeDeleteUserModal();
            loadUsers();
        } else {
            const err = await res.json();
            showToast(err.message || "Silinemedi", "error");
        }
    } catch (e) { showToast("Sunucu hatası", "error"); }
    finally {
        btn.innerText = originalText;
        btn.disabled = false;
    }
};

// ==========================================
// 6. ROZET YÖNETİMİ
// ==========================================

// Rozetleri Listele
window.loadBadges = async function () {
    const container = document.getElementById('badge-list-container');
    if (!container) return;

    container.innerHTML = '<tr><td colspan="4" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const response = await fetch(`${API_BASE_URL}/Badge`, { headers: getHeaders() });
        if (!response.ok) throw new Error("Hata");

        const badges = await response.json();
        container.innerHTML = "";

        if (badges.length === 0) {
            container.innerHTML = '<tr><td colspan="4" style="text-align:center;">Henüz rozet yok.</td></tr>';
            return;
        }

        badges.forEach(badge => {
            const row = `
                <tr>
                    <td>
                        <img src="${badge.imageUrl}" alt="Rozet" 
                             style="width: 40px; height: 40px; object-fit: contain; background:#f3f4f6; border-radius:50%; padding:2px;">
                    </td>
                    <td style="font-weight:600; color:#333;">${badge.name}</td>
                    <td style="font-size:13px; color:#666;">${badge.description}</td>
                    <td style="font-size:11px; color:#999; font-family:monospace;">${badge.imageUrl}</td>
                </tr>`;
            container.innerHTML += row;
        });

    } catch (error) {
        container.innerHTML = `<tr><td colspan="4" style="text-align:center; color:red;">Yüklenemedi.</td></tr>`;
    }
}

// Rozet Ekleme Formu Dinleyicisi
document.addEventListener('DOMContentLoaded', () => {
    // ... Diğer listenerlar ...

    const badgeForm = document.getElementById('add-badge-form');
    if (badgeForm) {
        badgeForm.addEventListener('submit', async (e) => {
            e.preventDefault();

            const btn = badgeForm.querySelector('button[type="submit"]');
            const originalText = btn.innerHTML;
            btn.innerHTML = "Yükleniyor...";
            btn.disabled = true;

            // FormData Hazırla
            const formData = new FormData();
            formData.append("Name", document.getElementById("badge-name").value);
            formData.append("Description", document.getElementById("badge-desc").value);
            formData.append("ImageFile", document.getElementById("badge-file").files[0]);

            try {
                // DİKKAT: FormData gönderirken 'Content-Type' header'ı EKLEMİYORUZ.
                // Tarayıcı boundary'i kendi ayarlar. Sadece Token ekliyoruz.
                const token = localStorage.getItem("jwtToken");

                const response = await fetch(`${API_BASE_URL}/Badge`, {
                    method: "POST",
                    headers: {
                        "Authorization": `Bearer ${token}`
                        // Content-Type YOK!
                    },
                    body: formData
                });

                if (response.ok) {
                    showToast("Rozet başarıyla eklendi!", "success");
                    badgeForm.reset();
                    loadBadges(); // Listeyi yenile
                } else {
                    const errText = await response.text();
                    showToast("Hata oluştu: " + errText, "error");
                }
            } catch (error) {
                console.error(error);
                showToast("Sunucu hatası.", "error");
            } finally {
                btn.innerHTML = originalText;
                btn.disabled = false;
            }
        });
    }
});
// -- ARKADAŞLIK SİLME --
window.openDeleteFriendshipModal = function (id) {
    document.getElementById('delete-friendship-id-input').value = id;
    document.getElementById('delete-friendship-modal').classList.add('active');
}

window.closeDeleteFriendshipModal = function () {
    document.getElementById('delete-friendship-modal').classList.remove('active');
}

window.confirmDeleteFriendship = async function () {
    const id = document.getElementById('delete-friendship-id-input').value;
    const btn = document.querySelector('#delete-friendship-modal .btn-delete-confirm');
    const originalText = btn.innerText;

    btn.innerText = "Siliniyor...";
    btn.disabled = true;

    try {
        const res = await fetch(`${API_BASE_URL}/Admin/friendships/${id}`, {
            method: 'DELETE', headers: getHeaders()
        });

        if (res.ok) {
            showToast("Arkadaşlık silindi", "success");
            closeDeleteFriendshipModal();
            loadFriendships();
        } else {
            const err = await res.json();
            showToast(err.message || "Hata oluştu", "error");
        }
    } catch (e) { showToast("Sunucu hatası", "error"); }
    finally {
        btn.innerText = originalText;
        btn.disabled = false;
    }
};

// ==========================================
// 6. SİSTEM LOGLARI
// ==========================================
window.loadSystemLogs = async function () {
    const container = document.getElementById('log-list-container');
    if (!container) return;

    container.innerHTML = '<tr><td colspan="4" style="text-align:center;">Yükleniyor...</td></tr>';

    try {
        const response = await fetch(`${API_BASE_URL}/Admin/logs`, { headers: getHeaders() });
        if (!response.ok) throw new Error("Hata");

        const logs = await response.json();
        container.innerHTML = "";

        if (logs.length === 0) {
            container.innerHTML = '<tr><td colspan="4" style="text-align:center; color:#999;">Sistem temiz! Hiç hata kaydı yok. 🎉</td></tr>';
            return;
        }

        logs.forEach(log => {
            const dateStr = new Date(log.createdDate).toLocaleString('tr-TR');

            // Hata seviyesine göre renk
            let badgeClass = 'badge-success'; // Info
            if (log.level === 'Error') badgeClass = 'badge-danger';
            if (log.level === 'Warning') badgeClass = 'badge-warning';

            const row = `
                <tr>
                    <td style="font-size:12px; color:#666;">${dateStr}</td>
                    <td><span class="badge ${badgeClass}">${log.level}</span></td>
                    <td style="font-size:13px; font-family:monospace; color:#333;">${log.message.substring(0, 80)}...</td>
                    <td style="text-align:center;">
                        <button class="btn-icon" onclick="showLogDetails('${encodeURIComponent(log.stackTrace || 'Detay yok')}')" style="background:#e0e7ff; color:#4F46E5;">
                            <span class="material-icons" style="font-size:18px;">visibility</span>
                        </button>
                    </td>
                </tr>`;
            container.innerHTML += row;
        });

    } catch (error) {
        container.innerHTML = `<tr><td colspan="4" style="text-align:center; color:red;">Loglar alınamadı.</td></tr>`;
    }
}

// Log Detayını Alert ile Göster (İstersen Modal yapabilirsin)
window.showLogDetails = function (encodedTrace) {
    const trace = decodeURIComponent(encodedTrace);
    alert("HATA DETAYI:\n\n" + trace);
}