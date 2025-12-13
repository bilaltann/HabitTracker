// js/main.js

import { checkAuth } from './utils.js';
import { loadUserData } from './user.js';
import { loadHabits, setupHabitListeners } from './habits.js';
import { setupFriendSystem, loadFriendRequests, loadActiveFriends } from './friends.js';
// UI fonksiyonlarının hepsini import ediyoruz
import { renderCalendarPage, renderBadgesPage, renderLevelsPage } from './ui.js';

document.addEventListener("DOMContentLoaded", () => {
    // 1. Önce yetki kontrolü
    checkAuth();

    // 2. Temel verileri yükle
    loadUserData();
    loadHabits();

    // 3. Dinleyicileri (Butonlar, Menüler) kur
    setupEventListeners();
    setupHabitListeners();
    setupFriendSystem();
});

function setupEventListeners() {
    // --- NAVİGASYON MENÜSÜ ---
    document.querySelectorAll(".nav-item").forEach(item => {
        item.addEventListener("click", () => {
            // A. Aktif menü stilini güncelle
            document.querySelectorAll(".nav-item").forEach(nav => nav.classList.remove("active"));
            item.classList.add("active");

            // B. İlgili sayfayı göster, diğerlerini gizle
            const pageId = item.getAttribute("data-page");
            document.querySelectorAll("section").forEach(sec => sec.classList.add("hidden"));

            const target = document.getElementById(`page-${pageId}`);
            if (target) target.classList.remove("hidden");

            // C. Sayfaya özel verileri yükle (Lazy Loading)
            if (pageId === "requests") {
                loadFriendRequests();
            }
            if (pageId === "friends") {
                loadActiveFriends();
            }
            if (pageId === "calendar") {
                renderCalendarPage();
            }
            if (pageId === "badges") {
                renderBadgesPage(); // Rozetler sekmesine basınca çalışır
            }
            if (pageId === "levels") {
                renderLevelsPage();
            }
        });
    });

    // --- ÇIKIŞ BUTONU ---
    const logoutBtn = document.getElementById("logout-btn");
    if (logoutBtn) {
        logoutBtn.addEventListener("click", () => {
            localStorage.removeItem("jwtToken");
            localStorage.removeItem("habitQuestState"); // Varsa local state'i de temizle
            window.location.href = "login.html";
        });
    }
}