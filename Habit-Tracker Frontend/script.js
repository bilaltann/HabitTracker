
        // API Yapılandırması
        const API_BASE_URL = "https://localhost:7223/api";
        
        // Global Değişkenler (Diğer sayfalar erişebilsin diye)
        let currentHabits = []; 
        
        // --- SABİT VERİLER (LEVELS & BADGES) ---
        const LEVELS = [
            { level: 1, name: "Başlangıç", points: 0, icon: "image/level_1_tohum.png" },
            { level: 2, name: "Çırak", points: 10, icon: "image/level_2_cirak.png" },
            { level: 3, name: "Gözlemci", points: 20, icon: "image/level_3_gozlemci.png" },
            { level: 4, name: "Gezgin", points: 30, icon: "image/level_4_gezgin.png" },
            { level: 5, name: "Uyanış", points: 40, icon: "image/level_5_uyanis.png" },
            { level: 6, name: "Usta Adayı", points: 50, icon: "image/level_6_usta_adayi.png" },
            { level: 7, name: "Uzman", points: 60, icon: "image/level_7_uzman.png" },
            { level: 8, name: "Lider", points: 70, icon: "image/level_8_lider.png" },
            { level: 9, name: "Kıdemli", points: 80, icon: "image/level_9_kidemli.png" },
            { level: 10, name: "Zen Ustası", points: 90, icon: "image/level_10_zen_ustasi.png" }
        ];

        const BADGES = {
            'ILK_ADIM': { id: 'ILK_ADIM', name: "İlk Adım", icon: '👶', requirement: "İlk alışkanlığını ekle" },
            'ISTIKRAR': { id: 'ISTIKRAR', name: "İstikrar", icon: '🔥', requirement: "En az 1 alışkanlığı tamamla" },
            'UCA_KOS': { id: 'UCA_KOS', name: "Üçe Koş", icon: '🚀', requirement: "3 tane alışkanlığın olsun" },
            'USTA': { id: 'USTA', name: "Usta", icon: '👑', requirement: "50 puana ulaş" }
        };

        // --- BAŞLANGIÇ ---
        document.addEventListener("DOMContentLoaded", () => {
            checkAuth();
            try { loadUserData(); } catch (e) { console.error("User Data:", e); }
            try { loadHabits(); } catch (e) { console.error("Habits:", e); }
            setupEventListeners();
        });

        function checkAuth() {
            if (!localStorage.getItem("jwtToken")) window.location.href = "login.html";
        }

        function getAuthHeaders() {
            return {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${localStorage.getItem("jwtToken")}`
            };
        }

        // --- KULLANICI VERİLERİ ---
        function loadUserData() {
            let state = JSON.parse(localStorage.getItem("habitQuestState")) || { user: { points: 0, username: 'Kullanıcı' } };
            const points = state.user.points || 0;

            document.getElementById("user-display").textContent = `Merhaba, ${state.user.username || 'Kullanıcı'}!`;
            document.getElementById("user-points").textContent = points;

            // Seviye Hesaplama
            const currentLevelIndex = Math.min(Math.floor(points / 10), LEVELS.length - 1);
            const currentLevelData = LEVELS[currentLevelIndex];
            const nextLevelData = LEVELS[currentLevelIndex + 1];
            
            let progressPercent = 100;
            if(nextLevelData) {
                const pointsInLevel = points % 10;
                progressPercent = (pointsInLevel / 10) * 100;
                document.getElementById("progress-text").textContent = `Sonraki seviyeye: %${Math.round(progressPercent)}`;
            } else {
                document.getElementById("progress-text").textContent = "Maksimum Seviye!";
            }

            document.getElementById("user-level-name").textContent = `${currentLevelData.name} (Lv.${currentLevelData.level})`;
            document.getElementById("level-progress-bar").style.width = `${progressPercent}%`;
        }

        // --- ALIŞKANLIKLARI YÜKLE ---
        async function loadHabits() {
            const listContainer = document.getElementById("habit-list");
            if (listContainer) listContainer.innerHTML = '<p class="empty-message">Yükleniyor...</p>';

            try {
                const response = await fetch(`${API_BASE_URL}/Habit`, { method: "GET", headers: getAuthHeaders() });
                
                if (response.status === 401) { logout(); return; }
                if (!response.ok) throw new Error("API Hatası");

                currentHabits = await response.json(); // Global değişkene ata
                
                renderHabits(currentHabits);
                renderLevelsPage();   // Seviyeleri oluştur
                renderBadgesPage();   // Rozetleri hesapla
                renderCalendarPage(); // Takvimi çiz

            } catch (error) {
                console.error(error);
                if(listContainer) listContainer.innerHTML = `<p class="empty-message" style="color:red">Hata oluştu.</p>`;
            }
        }

        function renderHabits(habits) {
            const listContainer = document.getElementById("habit-list");
            if(!listContainer) return;
            listContainer.innerHTML = "";

            if (habits.length === 0) {
                listContainer.innerHTML = '<p class="empty-message">Henüz alışkanlığın yok.</p>';
                return;
            }

            habits.forEach(habit => {
                const isDone = habit.isCompletedToday;
                const btnStyle = isDone ? "background-color: #10B981; color: white;" : "background-color: #e0e7ff; color: #4F46E5;";
                const btnText = isDone ? '<span class="material-icons" style="font-size:16px;">check</span> Tamamlandı' : 'İşaretle';
                const freqText = (habit.frequency == 1) ? "Haftalık" : "Günlük";

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
                            <button onclick="toggleHabit(${habit.id}, ${isDone})" style="${btnStyle} border:none; padding:8px 15px; border-radius:8px; font-weight:600; cursor:pointer; display:flex; align-items:center; gap:5px;">${btnText}</button>
                            <button onclick="openEditModal(${habit.id}, '${habit.name}', '${habit.category}', ${habit.frequency})" style="background:#e0e7ff; color:#4F46E5; border:none; border-radius:8px; width:36px; height:36px; cursor:pointer; display:flex; align-items:center; justify-content:center;"><span class="material-icons">edit</span></button>
                            <button onclick="deleteHabit(${habit.id})" style="background:#fee2e2; color:#ef4444; border:none; width:36px; height:36px; border-radius:8px; cursor:pointer; display:flex; align-items:center; justify-content:center;"><span class="material-icons">delete</span></button>
                        </div>
                    </div>`;
                listContainer.innerHTML += itemHtml;
            });
        }

        // --- SAYFA RENDER FONKSİYONLARI (EKSİK OLANLAR) ---

        // 1. SEVİYELER SAYFASI
        function renderLevelsPage() {
            const container = document.getElementById("levels-container");
            if(!container) return;
            container.innerHTML = "";
            
            let state = JSON.parse(localStorage.getItem("habitQuestState")) || { user: { points: 0 } };
            const userPoints = state.user.points || 0;

            LEVELS.forEach(lvl => {
                const isUnlocked = userPoints >= lvl.points;
                const statusClass = isUnlocked ? "unlocked" : "locked";
                const borderStyle = isUnlocked ? "border: 2px solid var(--primary-color);" : "opacity: 0.6;";
                
                const card = document.createElement("div");
                card.className = "glass-panel";
                card.style.cssText = `text-align:center; padding:20px; ${borderStyle}`;
                
                card.innerHTML = `
                    <div style="font-size:40px; margin-bottom:10px;">${isUnlocked ? '🔓' : '🔒'}</div>
                    <h3>${lvl.name}</h3>
                    <p style="font-size:12px; color:#666;">Gereken Puan: ${lvl.points}</p>
                    <p style="font-weight:bold; color:${isUnlocked ? '#10B981' : '#666'}">${isUnlocked ? 'AÇILDI' : 'KİLİTLİ'}</p>
                `;
                container.appendChild(card);
            });
        }

        // 2. ROZETLER SAYFASI
        function renderBadgesPage() {
            const container = document.getElementById("badges-content-area-main");
            if(!container) return;
            container.innerHTML = "";

            let state = JSON.parse(localStorage.getItem("habitQuestState")) || { user: { points: 0 } };
            
            // Rozet kazanma mantığı (Basit kurallar)
            const unlockedBadges = [];
            if(currentHabits.length > 0) unlockedBadges.push('ILK_ADIM');
            if(currentHabits.length >= 3) unlockedBadges.push('UCA_KOS');
            if(currentHabits.some(h => h.isCompletedToday)) unlockedBadges.push('ISTIKRAR');
            if(state.user.points >= 50) unlockedBadges.push('USTA');

            Object.keys(BADGES).forEach(key => {
                const badge = BADGES[key];
                const isEarned = unlockedBadges.includes(key);
                
                const card = document.createElement("div");
                card.className = "glass-panel";
                card.style.cssText = `text-align:center; padding:15px; ${isEarned ? 'border: 2px solid #FFD700;' : 'opacity: 0.5;'}`;
                
                card.innerHTML = `
                    <div style="font-size:35px; margin-bottom:5px;">${badge.icon}</div>
                    <h4>${badge.name}</h4>
                    <p style="font-size:11px; color:#666;">${badge.requirement}</p>
                    <div style="margin-top:5px; font-weight:bold; font-size:12px; color:${isEarned ? '#F59E0B' : '#999'}">
                        ${isEarned ? 'KAZANILDI' : 'KİLİTLİ'}
                    </div>
                `;
                container.appendChild(card);
            });
        }

        // 3. TAKVİM SAYFASI
        function renderCalendarPage() {
            const container = document.getElementById("calendar-cards-container");
            if(!container) return;
            container.innerHTML = "";

            // Son 5 gün + Gelecek 2 gün
            for (let i = -4; i <= 2; i++) {
                const d = new Date();
                d.setDate(d.getDate() + i);
                const isToday = i === 0;
                
                // Basit gösterim: O gün yapılması gereken toplam alışkanlık sayısı
                // Not: Gerçek tamamlanma verisi için API'den Log tablosunu çekmek gerekir.
                // Şimdilik sadece aktif alışkanlık sayısını gösterelim.
                const totalHabits = currentHabits.length; 
                
                const card = document.createElement("div");
                card.className = "glass-panel";
                card.style.cssText = `padding:15px; text-align:center; ${isToday ? 'border: 2px solid var(--primary-color);' : ''}`;
                
                const dateStr = d.toLocaleDateString('tr-TR', { day: 'numeric', month: 'short', weekday: 'short' });
                
                card.innerHTML = `
                    <h4 style="margin-bottom:10px;">${dateStr} ${isToday ? '(Bugün)' : ''}</h4>
                    <p style="font-size:13px;">Hedef: ${totalHabits} Alışkanlık</p>
                    <div style="margin-top:5px;">${isToday ? '📝' : (i < 0 ? '✔️' : '⏳')}</div>
                `;
                container.appendChild(card);
            }
        }

        // --- İŞLEMLER (Ekle, Sil, Toggle, Edit) ---
        
        document.getElementById("add-habit-form").addEventListener("submit", async (e) => {
            e.preventDefault();
            const name = document.getElementById("habit-name").value;
            const category = document.getElementById("habit-category").value;
            const frequencyId = parseInt(document.querySelector('input[name="frequency"]:checked').value);

            try {
                const response = await fetch(`${API_BASE_URL}/Habit`, {
                    method: "POST", headers: getAuthHeaders(),
                    body: JSON.stringify({ name, category, frequencyId, userId: 0 })
                });
                if (response.ok) {
                    document.getElementById("add-habit-form").reset();
                    loadHabits();
                    showToast("Alışkanlık eklendi!", "success");
                }
            } catch (error) { console.error(error); }
        });

        async function deleteHabit(id) {
            if (!confirm("Silmek istediğinize emin misiniz?")) return;
            try {
                const response = await fetch(`${API_BASE_URL}/Habit/${id}`, { method: "DELETE", headers: getAuthHeaders() });
                if (response.ok) { loadHabits(); showToast("Silindi.", "success"); }
            } catch (e) { console.error(e); }
        }

        async function toggleHabit(id, wasCompleted) {
            const today = new Date();
            const dateStr = `${today.getFullYear()}-${String(today.getMonth()+1).padStart(2,'0')}-${String(today.getDate()).padStart(2,'0')}T00:00:00`;
            
            try {
                const response = await fetch(`${API_BASE_URL}/Habit/toggle`, {
                    method: "POST", headers: getAuthHeaders(),
                    body: JSON.stringify({ habitId: id, date: dateStr })
                });
                if (response.ok) {
                    updateLocalPoints(wasCompleted ? -1 : 1);
                    loadHabits();
                    showToast(wasCompleted ? "Geri alındı." : "Tamamlandı! (+1 Puan)", "success");
                }
            } catch (e) { console.error(e); }
        }

        function updateLocalPoints(amount) {
            let state = JSON.parse(localStorage.getItem("habitQuestState")) || { user: { points: 0 } };
            state.user.points = (state.user.points || 0) + amount;
            if(state.user.points < 0) state.user.points = 0;
            localStorage.setItem("habitQuestState", JSON.stringify(state));
            loadUserData();
        }

        // --- EDIT MODAL ---
        function openEditModal(id, name, category, freq) {
            const modal = document.getElementById("edit-modal");
            document.getElementById("edit-habit-id").value = id;
            document.getElementById("edit-habit-name").value = name;
            document.getElementById("edit-habit-category").value = category;
            const radios = document.getElementsByName("edit-frequency");
            const freqVal = (freq == 1) ? "1" : "0";
            for(const r of radios) { if(r.value === freqVal) r.checked = true; }
            modal.classList.add("active");
        }
        function closeEditModal() { document.getElementById("edit-modal").classList.remove("active"); }

        if(document.getElementById("edit-habit-form")) {
            document.getElementById("edit-habit-form").addEventListener("submit", async (e) => {
                e.preventDefault();
                const id = document.getElementById("edit-habit-id").value;
                const name = document.getElementById("edit-habit-name").value;
                const category = document.getElementById("edit-habit-category").value;
                const freq = parseInt(document.querySelector('input[name="edit-frequency"]:checked').value);

                try {
                    const response = await fetch(`${API_BASE_URL}/Habit`, {
                        method: "PUT", headers: getAuthHeaders(),
                        body: JSON.stringify({ id: parseInt(id), name, category, frequencyId: freq, isActive: true })
                    });
                    if(response.ok) { closeEditModal(); loadHabits(); showToast("Güncellendi!", "success"); }
                } catch(e) { console.error(e); }
            });
        }

        // --- NAVİGASYON VE DİĞER ---
        document.querySelectorAll(".nav-item").forEach(item => {
            item.addEventListener("click", () => {
                document.querySelectorAll(".nav-item").forEach(nav => nav.classList.remove("active"));
                item.classList.add("active");
                const pageId = item.getAttribute("data-page");
                document.querySelectorAll("section").forEach(sec => sec.classList.add("hidden"));
                const target = document.getElementById(`page-${pageId}`);
                if(target) target.classList.remove("hidden");
            });
        });

        document.getElementById("logout-btn").addEventListener("click", () => {
            localStorage.removeItem("jwtToken");
            localStorage.removeItem("habitQuestState");
            window.location.href = "login.html";
        });

        function showToast(message, type = 'success') {
            let container = document.getElementById('toast-container');
            if (!container) { container = document.createElement('div'); container.id = 'toast-container'; document.body.appendChild(container); }
            const iconName = type === 'success' ? 'check_circle' : 'error';
            const toast = document.createElement('div');
            toast.className = `toast ${type}`;
            toast.innerHTML = `<div class="toast-content"><span class="material-icons toast-icon">${iconName}</span><span class="toast-message">${message}</span></div>`;
            toast.onclick = function() { this.remove(); };
            container.appendChild(toast);
            setTimeout(() => { toast.remove(); }, 3000);
        }
