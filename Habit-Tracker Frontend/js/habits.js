import { API_BASE_URL } from './config.js';
import { getAuthHeaders, showToast } from './utils.js';
import { updateLocalPoints } from './user.js';
import { renderLevelsPage, renderBadgesPage, renderCalendarPage } from './ui.js';

// Dışarıdan erişilebilecek değişkenler
export let currentHabits = [];

// Filtreleme için tüm veriyi hafızada tutacağımız değişken
let allHabitsCache = [];


//ALIŞKANLIKLARI YÜKLEME

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

        const data = await response.json();

        // Veriyi kaydet
        currentHabits = data;
        allHabitsCache = data;

        //Sayfa ilk açıldığında filtreleri temizle
        resetFilters();

        //Listeyi olduğu gibi ekrana bas
        renderHabits(allHabitsCache);

        // UI'ın diğer parçalarını güncelle
        renderLevelsPage();
        renderBadgesPage();
        renderCalendarPage();

    } catch (error) {
        console.error(error);
        if (listContainer) listContainer.innerHTML = `<p class="empty-message" style="color:red">Bağlantı hatası.</p>`;
    }
}


// 2. FİLTRELEME

export function filterHabits() {
    const searchInput = document.getElementById("filter-search");
    const categorySelect = document.getElementById("filter-category");
    const frequencySelect = document.getElementById("filter-frequency");

    // Eğer bu elementler sayfada yoksa (örn: başka sayfadaysak) dur.
    if (!searchInput || !categorySelect || !frequencySelect) return;

    const searchTerm = searchInput.value.toLowerCase();
    const selectedCategory = categorySelect.value;
    const selectedFrequency = frequencySelect.value;

    // Önbellekteki (allHabitsCache) tüm verileri süzgeçten geçir
    const filtered = allHabitsCache.filter(habit => {
        
        const nameMatch = habit.name.toLowerCase().includes(searchTerm);

        
        const categoryMatch = selectedCategory === "all" || habit.category === selectedCategory;

        
        let freqString = habit.frequency.toString();
        if (habit.frequency === 0) freqString = "Daily";
        if (habit.frequency === 1) freqString = "Weekly";

        const frequencyMatch = selectedFrequency === "all" || freqString === selectedFrequency;

        // Hepsi uyuyorsa TRUE döner ve listeye eklenir
        return nameMatch && categoryMatch && frequencyMatch;
    });

    // Filtrelenmiş listeyi ekrana çiz
    renderHabits(filtered);
}

// Filtreleri temizleyen yardımcı fonksiyon
function resetFilters() {
    const searchInp = document.getElementById("filter-search");
    const catSelect = document.getElementById("filter-category");
    const freqSelect = document.getElementById("filter-frequency");

    if (searchInp) searchInp.value = "";
    if (catSelect) catSelect.value = "all";
    if (freqSelect) freqSelect.value = "all";
}




function renderHabits(habits) {
    const listContainer = document.getElementById("habit-list");
    if (!listContainer) return;

    listContainer.innerHTML = "";

    if (habits.length === 0) {
        listContainer.innerHTML = '<p class="empty-message">Aradığınız kriterlere uygun alışkanlık bulunamadı.</p>';
        return;
    }

    habits.forEach(habit => {
        const isDone = habit.isCompletedToday;

        // Tamamlandı/Tamamlanmadı buton stili
        const btnStyle = isDone
            ? "background-color: #10B981; color: white;"
            : "background-color: #e0e7ff; color: #4F46E5;";

        const btnText = isDone
            ? '<span class="material-icons" style="font-size:16px;">check</span> Tamamlandı'
            : 'İşaretle';

        // Frekans ve Metin Ayarları
        const isWeekly = (habit.frequency === "Weekly" || habit.frequency === 1);
        const freqText = isWeekly ? "Haftalık" : "Günlük";
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
                    <!-- 1. Tamamla Butonu -->
                    <button onclick="toggleHabit(${habit.id}, ${isDone})" 
                            style="${btnStyle} border:none; padding:8px 15px; border-radius:8px; font-weight:600; cursor:pointer; display:flex; align-items:center; gap:5px;">
                        ${btnText}
                    </button>
                    
                    <!-- 2. Düzenle Butonu -->
                    <button onclick="openEditModal(${habit.id}, '${safeName}', '${safeCategory}', ${freqId})" 
                            style="background:#e0e7ff; color:#4F46E5; border:none; border-radius:8px; width:36px; height:36px; cursor:pointer; display:flex; align-items:center; justify-content:center; margin-left:5px;">
                        <span class="material-icons">edit</span>
                    </button>

                    <!-- 3. YENİ EKLENEN: Paylaş Butonu -->
                    <button onclick="openShareModal(${habit.id})" title="Arkadaşınla Paylaş"
                            style="background:#e0f2fe; color:#0284c7; border:none; width:36px; height:36px; border-radius:8px; cursor:pointer; display:flex; align-items:center; justify-content:center; margin-left:5px;">
                        <span class="material-icons">share</span>
                    </button>
                    
                    <!-- 4. Sil Butonu -->
                    <button onclick="deleteHabit(${habit.id})" 
                            style="background:#fee2e2; color:#ef4444; border:none; width:36px; height:36px; border-radius:8px; cursor:pointer; display:flex; align-items:center; justify-content:center; margin-left:5px;">
                        <span class="material-icons">delete</span>
                    </button>
                </div>
            </div>`;
        listContainer.innerHTML += itemHtml;
    });
}

// 4. GLOBAL FONKSİYONLAR (WINDOW)
// (HTML'den onclick ile çağrılanlar)


window.deleteHabit = function (id) {
    const modal = document.getElementById("delete-modal");
    document.getElementById("delete-habit-id-input").value = id;
    modal.classList.add("active");
};

window.confirmDelete = async function () {
    const id = document.getElementById("delete-habit-id-input").value;
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
            loadHabits();
            showToast("Alışkanlık silindi.", "success");
            window.closeDeleteModal();
        } else {
            showToast("Silme işlemi başarısız.", "error");
        }
    } catch (e) {
        console.error(e);
        showToast("Sunucu hatası.", "error");
    } finally {
        confirmBtn.innerText = originalText;
        confirmBtn.disabled = false;
    }
};

window.closeDeleteModal = function () {
    const modal = document.getElementById("delete-modal");
    if (modal) modal.classList.remove("active");
};

// --- TÜMÜNÜ SİLME İŞLEMLERİ ---

// 1. Kullanıcıdan onay isteyen fonksiyon
function confirmDeleteAll() {
    // Tarayıcının varsayılan onay kutusunu kullanıyoruz
    const answer = confirm("DİKKAT! Tüm alışkanlıklarınız kalıcı olarak silinecek.\n\nBu işlem geri alınamaz. Emin misiniz?");

    if (answer) {
        deleteAllHabits();
    }
}

// 2. API'ye istek atan fonksiyon
async function deleteAllHabits() {
    try {
        const response = await fetch(`${API_BASE_URL}/Habit/delete-all`, {
            method: "DELETE",
            headers: {
                // Token'ı header'a ekliyoruz ki backend hangi kullanıcı olduğunu bilsin
                "Authorization": `Bearer ${localStorage.getItem("jwtToken")}`,
                "Content-Type": "application/json"
            }
        });

        if (response.ok) {
            // Başarılı olursa:
            showToast("Tüm alışkanlıklar başarıyla silindi.", "success");

            // 1. Listeyi yenile (Boş gelecek)
            renderHabits([]);

            // 2. İstatistikleri güncelle (Puan, seviye vs. düşebilir veya sıfırlanabilir)
            updateDashboard();
        } else {
            showToast("Silme işlemi sırasında bir hata oluştu.", "error");
        }

    }
    catch (error) {
    }
}

// Fonksiyonu dışarıya (window nesnesine) açıyoruz ki HTML erişebilsin
window.confirmDeleteAll = confirmDeleteAll;

window.toggleHabit = async function (id, wasCompleted) {
    const today = new Date();
    // Tarih formatı: YYYY-MM-DDT00:00:00
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

window.openEditModal = function (id, name, category, freq) {
    const modal = document.getElementById("edit-modal");

    document.getElementById("edit-habit-id").value = id;
    document.getElementById("edit-habit-name").value = name;
    document.getElementById("edit-habit-category").value = category;

    const radios = document.getElementsByName("edit-frequency");
    // Gelen freq verisini stringe çevirip kontrol et
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
                btn.textContent = originalText;
                btn.disabled = false;
            }
        });
    }

    // FİLTRE 
    // Arama kutusuna her harf yazıldığında veya seçimler değiştiğinde tetiklenir
    const searchInput = document.getElementById("filter-search");
    const categorySelect = document.getElementById("filter-category");
    const frequencySelect = document.getElementById("filter-frequency");

    if (searchInput) {
        searchInput.addEventListener("input", filterHabits);
    }
    if (categorySelect) {
        categorySelect.addEventListener("change", filterHabits);
    }
    if (frequencySelect) {
        frequencySelect.addEventListener("change", filterHabits);
    }
}