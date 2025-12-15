import { API_BASE_URL, LEVELS } from './config.js'; 
import { getAuthHeaders } from './utils.js';
import { currentHabits } from './habits.js'; 
import { initCalendar } from './calendarPage.js';


export function renderLevelsPage() {
    const container = document.getElementById("levels-container");
    if (!container) return;
    container.innerHTML = "";
    let state = JSON.parse(localStorage.getItem("habitQuestState")) || { user: { points: 0 } };
    const userPoints = state.user.points || 0;
    LEVELS.forEach(lvl => {
        const isUnlocked = userPoints >= lvl.points;
        const borderStyle = isUnlocked ? "border: 2px solid var(--primary-color);" : "opacity: 0.6;";
        const card = document.createElement("div");
        card.className = "glass-panel";
        card.style.cssText = `text-align:center; padding:20px; ${borderStyle}`;
        card.innerHTML = `<div style="font-size:40px; margin-bottom:10px;">${isUnlocked ? '🔓' : '🔒'}</div><h3>${lvl.name}</h3><p style="font-size:12px; color:#666;">${lvl.points} Puan</p>`;
        container.appendChild(card);
    });
}

export async function renderBadgesPage() {
    const container = document.getElementById("badges-content-area-main");
    if (!container) return;

    container.innerHTML = '<p class="empty-message">Rozetler yükleniyor...</p>';

    try {
        // Tüm rozetleri çek
        const allBadgesResponse = await fetch(`${API_BASE_URL}/Badge`, {
            method: "GET", headers: getAuthHeaders()
        });

        // Kullanıcının kazandığı rozet ID'lerini çek
        const earnedResponse = await fetch(`${API_BASE_URL}/Badge/earned`, {
            method: "GET", headers: getAuthHeaders()
        });

        if (!allBadgesResponse.ok || !earnedResponse.ok) throw new Error("Veri hatası");

        const allBadges = await allBadgesResponse.json();
        const earnedIds = await earnedResponse.json(); 

        if (allBadges.length === 0) {
            container.innerHTML = '<p class="empty-message">Henüz tanımlı rozet yok.</p>';
            return;
        }

        container.innerHTML = "";

        // Listeleme ve Kontrol
        allBadges.forEach(badge => {
            // Bu rozetin ID'si kazanılanlar listesinde var mı?
            const isEarned = earnedIds.includes(badge.id);

            // Duruma göre stil belirle
            const cardClass = isEarned ? "badge-unlocked" : "badge-locked";
            const statusText = isEarned ?
                `<span style="color:#10B981; font-weight:bold; font-size:12px;">✅ KAZANILDI</span>` :
                `<span style="color:#666; font-size:12px;">🔒 KİLİTLİ</span>`;

            // Eğer kilitliyse üzerine kilit ikonu koyalım
            const lockIcon = isEarned ? "" : `<span class="material-icons lock-icon">lock</span>`;

            const card = document.createElement("div");
            card.className = `glass-panel ${cardClass}`;
            card.style.cssText = `text-align:center; padding:15px; position:relative; transition:0.3s;`;

            card.innerHTML = `
                ${lockIcon}
                <div style="margin-bottom:10px;">
                    <!-- Veritabanındaki ImageUrl (image/rozet.png) direkt kullanılır -->
                    <img src="${badge.imageUrl}" alt="${badge.name}" style="width: 70px; height: 70px; object-fit: contain;">
                </div>
                <h4 style="margin:5px 0;">${badge.name}</h4>
                <p style="font-size:11px; color:#555; margin-bottom:10px; min-height:30px;">${badge.description}</p>
                <div>${statusText}</div>
            `;

            container.appendChild(card);
        });

    } catch (error) {
        console.error(error);
        container.innerHTML = '<p class="empty-message" style="color:red">Rozetler yüklenemedi.</p>';
    }
}

export function renderCalendarPage() {
    // FullCalendar'ı başlat 
    setTimeout(() => {
        initCalendar();
    }, 100);
}