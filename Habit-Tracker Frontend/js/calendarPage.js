// js/calendarPage.js

import { currentHabits } from './habits.js';

// Takvimi Başlatma Fonksiyonu
export function initCalendar() {
    $('#calendar').fullCalendar('destroy');
    $('#calendar').fullCalendar({
        header: {
            left: 'prev,next today',
            center: 'title',
            right: 'month,basicWeek'
        },
        locale: 'tr', // Türkçe
        defaultDate: new Date(),
        navLinks: false,
        editable: false,
        eventLimit: true,
        events: generateEventsFromHabits(), // Çubukları oluştur

        // 1. BOŞ BİR GÜNE TIKLANINCA
        dayClick: function (date, jsEvent, view) {
            openDayModal(date);
        },

        // 2. ETKİNLİĞE TIKLANINCA
        eventClick: function (calEvent, jsEvent, view) {
            openDayModal(calEvent.start);
        }
    });
}

// Alışkanlıkları Takvim Verisine Çevirir
function generateEventsFromHabits() {
    const events = [];

    currentHabits.forEach(habit => {
        // Başlangıç (createdDate yoksa Bugün)
        let startDate = habit.createdDate ? new Date(habit.createdDate) : new Date();

        // Saati sıfırla
        startDate.setHours(0, 0, 0, 0);

        let endDate = new Date(habit.expirationDate);
        endDate.setHours(0, 0, 0, 0);

        // Haftalık/Günlük Kontrolü
        const isWeekly = (habit.frequency == 1 || habit.frequency === "Weekly");
        const color = isWeekly ? '#10B981' : '#4F46E5'; // Yeşil / Mor

        // Döngü ile gün gün ilerle
        for (let d = new Date(startDate); d <= endDate; d.setDate(d.getDate() + 1)) {

            // --- DÜZELTME BURADA ---
            // .toISOString() yerine Yerel Tarihi (Local Date) kullanıyoruz.
            // Böylece saat farkından dolayı bir önceki güne kayma sorunu çözülüyor.
            const year = d.getFullYear();
            const month = String(d.getMonth() + 1).padStart(2, '0'); // Ayı 2 haneli yap (01, 02...)
            const day = String(d.getDate()).padStart(2, '0'); // Günü 2 haneli yap
            const localDateString = `${year}-${month}-${day}`;

            events.push({
                id: habit.id,
                title: habit.name,
                start: localDateString, // Düzeltilmiş tarih
                color: color,
                allDay: true
            });
        }
    });

    return events;
}

// --- MODAL İŞLEMLERİ ---

function openDayModal(dateObj) {
    const modal = document.getElementById('day-detail-modal');
    const title = document.getElementById('modal-date-title');
    const list = document.getElementById('day-habits-list');

    // Tarihi Formatla
    const dateStr = dateObj.format('D MMMM YYYY dddd');
    title.innerText = dateStr;

    // Seçilen Tarihi JS Objesine Çevir
    const clickedDate = dateObj.toDate();
    clickedDate.setHours(0, 0, 0, 0);

    // Filtreleme: Sadece o tarihte geçerli olanları göster
    const activeHabits = currentHabits.filter(h => {
        const start = h.createdDate ? new Date(h.createdDate) : new Date();
        start.setHours(0, 0, 0, 0);

        const end = new Date(h.expirationDate);
        end.setHours(0, 0, 0, 0);

        // Tarih aralığını kontrol et
        return clickedDate >= start && clickedDate <= end;
    });

    // Listeyi HTML'e Dök
    list.innerHTML = "";

    if (activeHabits.length === 0) {
        list.innerHTML = `
            <div style="text-align:center; padding: 20px;">
                <span class="material-icons" style="font-size: 40px; color: #ddd;">event_busy</span>
                <p style="color:#999; margin-top:10px;">Bu tarih için planlanmış bir alışkanlık yok.</p>
            </div>`;
    } else {
        activeHabits.forEach(h => {
            const isWeekly = (h.frequency == 1 || h.frequency === "Weekly");

            const freq = isWeekly ? "Haftalık" : "Günlük";
            const icon = isWeekly ? "date_range" : "today";
            const badgeColor = isWeekly ? "#ecfdf5" : "#eef2ff";
            const textColor = isWeekly ? "#047857" : "#4338ca";

            list.innerHTML += `
                <div style="display: flex; justify-content: space-between; align-items: center; padding: 12px; background: rgba(255,255,255,0.6); border: 1px solid #eee; border-radius: 10px; margin-bottom: 8px;">
                    <div style="display: flex; align-items: center; gap: 10px;">
                        <div style="background: ${badgeColor}; width: 40px; height: 40px; border-radius: 8px; display: flex; align-items: center; justify-content: center; color: ${textColor};">
                            <span class="material-icons">${icon}</span>
                        </div>
                        <div>
                            <h4 style="margin: 0; font-size: 15px; color: #333;">${h.name}</h4>
                            <span style="font-size: 12px; color: #666;">${h.category}</span>
                        </div>
                    </div>
                    <span style="font-size: 11px; font-weight: bold; background: ${badgeColor}; color: ${textColor}; padding: 4px 8px; border-radius: 20px;">
                        ${freq}
                    </span>
                </div>
            `;
        });
    }

    modal.classList.add('active');
}

window.closeDayModal = function () {
    const modal = document.getElementById('day-detail-modal');
    if (modal) modal.classList.remove('active');
};