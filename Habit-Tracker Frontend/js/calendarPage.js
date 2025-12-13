// js/calendarPage.js

import { currentHabits } from './habits.js';

// Takvimi Başlatma Fonksiyonu
export function initCalendar() {
    // jQuery ile FullCalendar'ı başlat
    $('#calendar').fullCalendar('destroy'); // Varsa eskisini temizle
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

        // 2. VAR OLAN BİR ETKİNLİĞE (ÇUBUĞA) TIKLANINCA
        eventClick: function (calEvent, jsEvent, view) {
            // Etkinliğin tarihini alıp yine aynı modalı açıyoruz
            openDayModal(calEvent.start);
        }
    });
}

// Alışkanlıkları Takvim Verisine Çevirir (Renkli Çubuklar)
function generateEventsFromHabits() {
    const events = [];

    currentHabits.forEach(habit => {
        // Alışkanlığın başlangıç tarihi (CreatedDate yoksa bugünü baz alıyoruz)
        // Eğer backend'den 'createdDate' geliyorsa burayı değiştir: new Date(habit.createdDate)
        let startDate = new Date();

        // Bitiş tarihi
        let endDate = new Date(habit.expirationDate);

        // Renk (Haftalık ise Yeşil, Günlük ise Mor)
        let color = habit.frequency == 1 ? '#10B981' : '#4F46E5';

        events.push({
            id: habit.id,
            title: habit.name,
            start: startDate.toISOString().split('T')[0],
            end: endDate.toISOString().split('T')[0],
            color: color,
            allDay: true
        });
    });

    return events;
}

// --- MODAL İŞLEMLERİ ---

function openDayModal(dateObj) {
    const modal = document.getElementById('day-detail-modal');
    const title = document.getElementById('modal-date-title');
    const list = document.getElementById('day-habits-list');

    // 1. Tarihi Formatla (Örn: 12 Aralık 2025 Cuma)
    const dateStr = dateObj.format('D MMMM YYYY dddd');
    title.innerText = dateStr;

    // 2. Seçilen Tarihi JS Date Objesine Çevir (Saatleri sıfırla)
    const clickedDate = dateObj.toDate();
    clickedDate.setHours(0, 0, 0, 0);

    // 3. O tarihte geçerli olan alışkanlıkları bul
    const activeHabits = currentHabits.filter(h => {
        // Başlangıç (Burada createdDate kullanmak daha doğru olur, şimdilik bugün diyoruz)
        const start = new Date();
        start.setHours(0, 0, 0, 0);

        // Bitiş
        const end = new Date(h.expirationDate);
        end.setHours(0, 0, 0, 0);

        // Tıklanan tarih bu aralıkta mı?
        // (FullCalendar bitiş tarihini dahil etmez, o yüzden <= yerine < kullanmak gerekebilir duruma göre)
        // Ancak bizim mantığımızda expirationDate dahil olsun istiyoruz.
        return clickedDate >= start && clickedDate <= end;
    });

    // 4. Listeyi HTML'e Dök
    list.innerHTML = "";

    if (activeHabits.length === 0) {
        list.innerHTML = `
            <div style="text-align:center; padding: 20px;">
                <span class="material-icons" style="font-size: 40px; color: #ddd;">event_busy</span>
                <p style="color:#999; margin-top:10px;">Bu tarih için planlanmış bir alışkanlık yok.</p>
            </div>`;
    } else {
        activeHabits.forEach(h => {
            const freq = h.frequency == 1 ? "Haftalık" : "Günlük";
            const icon = h.frequency == 1 ? "date_range" : "today";
            const badgeColor = h.frequency == 1 ? "#ecfdf5" : "#eef2ff"; // Yeşilimsi / Mavimsi arka plan
            const textColor = h.frequency == 1 ? "#047857" : "#4338ca";

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

    // 5. Modalı Aç
    modal.classList.add('active');
}

// Modalı Kapat (HTML'den erişilsin diye window'a atıyoruz)
window.closeDayModal = function () {
    document.getElementById('day-detail-modal').classList.remove('active');
};