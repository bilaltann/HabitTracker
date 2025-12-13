// js/habits.js

import { API_BASE_URL } from './config.js';
import { getAuthHeaders, showToast } from './utils.js';
import { updateLocalPoints } from './user.js';
import { renderLevelsPage, renderBadgesPage, renderCalendarPage } from './ui.js';

// Başka dosyalardan erişilebilsin diye export ediyoruz
export let currentHabits = [];

// ==========================================
// 1. ALIŞKANLIKLARI YÜKLEME VE LİSTELEME
// ==========================================
export async function loadHabits() {
    const listContainer = document.getElementById("habit-list");
    if (listContainer) listContainer.innerHTML = '<p class="empty-message">Yükleniyor...</p>';

    try {
        const response = await fetch(`${API_BASE_URL}/Habit`, {
            method: "GET",
            headers: getAuthHeaders()
        });

        if (response.status === 401) {
            localStorage.removeItem("jwtToken");
            window.location.href = "login.html";
            return;
        }

        if (!response.ok) throw new Error("API Hatası");

        currentHabits = await response.json();

        renderHabits(currentHabits);

        // UI güncellemeleri
        renderLevelsPage();
        renderBadgesPage();
        renderCalendarPage();

    } catch (error) {
        console.error(error);
        if (listContainer) listContainer.innerHTML = `<p class="empty-message" style="color:red">Bağlantı hatası.</p>`;
    }
}

function renderHabits(habits) {
    const listContainer = document.getElementById("habit-list");
    if (!listContainer) return;
    listContainer.innerHTML = "";

    if (habits.length === 0) {
        listContainer.innerHTML = '<p class="empty-message">Henüz alışkanlığın yok.</p>';
        return;
    }

    habits.forEach(habit => {
        const isDone = habit.isCompletedToday;

        const btnStyle = isDone
            ? "background-color: #10B981; color: white;"
            : "background-color: #e0e7ff; color: #4F46E5;";

        const btnText = isDone
            ? '<span class="material-icons" style="font-size:16px;">check</span> Tamamlandı'
            : 'İşaretle';

        // Görünüm için metin (Backend'den İngilizce gelse bile Türkçe yapıyoruz)
        // Eğer backend "Daily" veya "0" gönderiyorsa kontrol ediyoruz
        const isWeekly = (habit.frequency === "Weekly" || habit.frequency === "Haftalık" || habit.frequency == 1);
        const freqText = isWeekly ? "Haftalık" : "Günlük";

        // --- HATA DÜZELTME KISMI ---
        // Edit modalına gönderilecek ID'yi sayıya (0 veya 1) çeviriyoruz.
        // Böylece ReferenceError: Daily is not defined hatası çözülür.
        const freqId = isWeekly ? 1 : 0;

        // İsimlerin içinde tek tırnak varsa kodun kırılmaması için escape (kaçış) işlemi
        const safeName = habit.name ? habit.name.replace(/'/g, "\\'") : "";
        const safeCategory = habit.category ? habit.category.replace(/'/g, "\\'") : "";

        const itemHtml = `
            <div class="habit-item">
                <div class="habit-info">
                    <h4>${habit.name}</h4>
                    <div class="habit-meta">
                        <span class="habit-tag">${habit.category}</span>
                        <span class="habit-tag">${freqText}</span>
                    </div>
                </div>
                <div class="habit-actions">
                    <button onclick="toggleHabit(${habit.id}, ${isDone})" 
                            style="${btnStyle} border:none; padding:8px 15px; border-radius:8px; font-weight:600; cursor:pointer; display:flex; align-items:center; gap:5px;">
                        ${btnText}
                    </button>
                    
                    <!-- BURADA ARTIK freqId KULLANIYORUZ (Sayı olduğu için tırnak gerekmez) -->
                    <button onclick="openEditModal(${habit.id}, '${safeName}', '${safeCategory}', ${freqId})" 
                            style="background:#e0e7ff; color:#4F46E5; border:none; border-radius:8px; width:36px; height:36px; cursor:pointer; display:flex; align-items:center; justify-content:center; margin-left:5px;">
                        <span class="material-icons">edit</span>
                    </button>
                    
                    <button onclick="deleteHabit(${habit.id})" 
                            style="background:#fee2e2; color:#ef4444; border:none; width:36px; height:36px; border-radius:8px; cursor:pointer; display:flex; align-items:center; justify-content:center; margin-left:5px;">
                        <span class="material-icons">delete</span>
                    </button>
                </div>
            </div>`;
        listContainer.innerHTML += itemHtml;
    });
}
// ==========================================
// 2. GLOBAL FONKSİYONLAR (WINDOW)
// ==========================================

// js/habits.js içine...

// ==========================================
// 1. SİLME İŞLEMİ (GÜNCELLENMİŞ)
// ==========================================

// Eskiden direkt siliyordu, şimdi sadece MODALI AÇIYOR
window.deleteHabit = function (id) {
    const modal = document.getElementById("delete-modal");

    // Silinecek ID'yi gizli input'a kaydet ki sonra silebilelim
    document.getElementById("delete-habit-id-input").value = id;

    // Modalı göster
    modal.classList.add("active");
};

// Modal üzerindeki "Evet, Sil" butonuna basınca çalışan asıl fonksiyon
window.confirmDelete = async function () {
    // Gizli inputtan ID'yi al
    const id = document.getElementById("delete-habit-id-input").value;

    // Butonu pasif yap (Çift tıklamayı önle)
    const confirmBtn = document.querySelector(".btn-delete-confirm");
    const originalText = confirmBtn.innerText;
    confirmBtn.innerText = "Siliniyor...";
    confirmBtn.disabled = true;

    try {
        const response = await fetch(`${API_BASE_URL}/Habit/${id}`, {
            method: "DELETE",
            headers: getAuthHeaders()
        });

        if (response.ok) {
            loadHabits(); // Listeyi yenile
            showToast("Alışkanlık silindi.", "success");
            window.closeDeleteModal(); // Modalı kapat
        } else {
            showToast("Silme işlemi başarısız.", "error");
        }
    } catch (e) {
        console.error(e);
        showToast("Sunucu hatası.", "error");
    } finally {
        // Butonu eski haline getir
        confirmBtn.innerText = originalText;
        confirmBtn.disabled = false;
    }
};

// Silme Modalını Kapat
window.closeDeleteModal = function () {
    const modal = document.getElementById("delete-modal");
    if (modal) modal.classList.remove("active");
};
window.toggleHabit = async function (id, wasCompleted) {
    const today = new Date();
    const dateStr = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}T00:00:00`;

    try {
        const response = await fetch(`${API_BASE_URL}/Habit/toggle`, {
            method: "POST",
            headers: getAuthHeaders(),
            body: JSON.stringify({ habitId: id, date: dateStr })
        });

        if (response.ok) {
            updateLocalPoints(wasCompleted ? -1 : 1);
            loadHabits();
            showToast(wasCompleted ? "Geri alındı." : "Tamamlandı! (+1 Puan)", "success");
        }
    } catch (e) { console.error(e); }
};

// Modal Açma Fonksiyonu
window.openEditModal = function (id, name, category, freq) {
    const modal = document.getElementById("edit-modal");

    document.getElementById("edit-habit-id").value = id;
    document.getElementById("edit-habit-name").value = name;
    document.getElementById("edit-habit-category").value = category;

    // Frequency seçimi (Gelen veri int veya string olabilir, stringe çevirip kıyaslıyoruz)
    const radios = document.getElementsByName("edit-frequency");
    const freqVal = (freq == 1) ? "1" : "0";

    for (const r of radios) {
        if (r.value === freqVal) {
            r.checked = true;
        }
    }

    modal.classList.add("active");
};

window.closeEditModal = function () {
    const modal = document.getElementById("edit-modal");
    if (modal) modal.classList.remove("active");
};

// ==========================================
// 3. FORM LISTENERLARI
// ==========================================
export function setupHabitListeners() {

    // Ekleme Formu
    const addForm = document.getElementById("add-habit-form");
    if (addForm) {
        addForm.addEventListener("submit", async (e) => {
            e.preventDefault();
            const name = document.getElementById("habit-name").value;
            const category = document.getElementById("habit-category").value;
            const frequencyId = parseInt(document.querySelector('input[name="frequency"]:checked').value);

            try {
                const response = await fetch(`${API_BASE_URL}/Habit`, {
                    method: "POST",
                    headers: getAuthHeaders(),
                    body: JSON.stringify({ name, category, frequencyId, userId: 0 })
                });

                if (response.ok) {
                    addForm.reset();
                    loadHabits();
                    showToast("Alışkanlık eklendi!", "success");
                }
            } catch (error) { console.error(error); }
        });
    }

    // Düzenleme Formu
    const editForm = document.getElementById("edit-habit-form");
    if (editForm) {
        editForm.addEventListener("submit", async (e) => {
            e.preventDefault();

            // Butonu kilitle (Çift tıklamayı önlemek için)
            const btn = editForm.querySelector("button[type='submit']");
            const originalText = btn.textContent;
            btn.textContent = "Güncelleniyor...";
            btn.disabled = true;

            const id = document.getElementById("edit-habit-id").value;
            const name = document.getElementById("edit-habit-name").value;
            const category = document.getElementById("edit-habit-category").value;
            const freq = parseInt(document.querySelector('input[name="edit-frequency"]:checked').value);

            try {
                const response = await fetch(`${API_BASE_URL}/Habit/${id}`, {
                    method: "PUT",
                    headers: getAuthHeaders(),
                    body: JSON.stringify({
                        id: parseInt(id),
                        name: name,
                        category: category,
                        frequencyId: freq,
                        isActive: true
                    })
                });

                if (response.ok) {
                    window.closeEditModal();
                    loadHabits();
                    showToast("Güncellendi!", "success");
                } else {
                    const errorData = await response.json();
                    showToast(errorData.message || "Güncelleme başarısız.", "error");
                }
            } catch (e) {
                console.error(e);
                showToast("Sunucu hatası.", "error");
            } finally {
                // İşlem bitince butonu eski haline getir
                btn.textContent = originalText;
                btn.disabled = false;
            }
        });
    }
}