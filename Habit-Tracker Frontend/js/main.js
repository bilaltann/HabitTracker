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
    setupSettingsListeners();
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

// Şifre ve Email işlemleri için gerekli fonksiyon
function setupSettingsListeners() {

    //ŞİFRE DEĞİŞTİRME
    const passwordForm = document.getElementById("change-password-form");
    if (passwordForm) {
        passwordForm.addEventListener("submit", async (e) => {
            e.preventDefault();

            const currentPassword = document.getElementById("current-password").value;
            const newPassword = document.getElementById("new-password").value;
            const btn = passwordForm.querySelector("button");

            // Basit Validasyon
            if (newPassword.length < 8) {
                showToast("Yeni şifre en az 8 karakter olmalı.", "error");
                return;
            }

            const originalText = btn.textContent;
            btn.textContent = "İşleniyor...";
            btn.disabled = true;

            try {
                // Backend'deki ChangePasswordDto ile uyumlu veri yapısı
                const response = await fetch(`${API_BASE_URL}/Auth/change-password`, {
                    method: "PUT",
                    headers: getAuthHeaders(),
                    body: JSON.stringify({
                        currentPassword: currentPassword,
                        newPassword: newPassword
                    })
                });

                const result = await response.json();

                if (response.ok) {
                    showToast("Şifreniz başarıyla güncellendi! 🔒", "success");
                    passwordForm.reset();
                } else {
                    showToast(result.message || "Şifre değiştirilemedi.", "error");
                }
            } catch (error) {
                console.error(error);
                showToast("Sunucu hatası.", "error");
            } finally {
                btn.textContent = originalText;
                btn.disabled = false;
            }
        });
    }

    // E-POSTA GÜNCELLEME 
    const emailForm = document.getElementById("update-email-form");
    if (emailForm) {
        // Sayfa açıldığında "Mevcut E-posta" alanını otomatik dolduralım (Kullanıcı kolaylığı)
        const state = JSON.parse(localStorage.getItem("habitQuestState"));
        if (state && state.user && state.user.email) {
            document.getElementById("current-email-input").value = state.user.email;
        }

        emailForm.addEventListener("submit", async (e) => {
            e.preventDefault();

            const currentEmailInput = document.getElementById("current-email-input").value;
            const newEmailInput = document.getElementById("new-email-input").value;
            const btn = emailForm.querySelector("button");

            
            const storedState = JSON.parse(localStorage.getItem("habitQuestState"));
            if (storedState && storedState.user && storedState.user.email !== currentEmailInput) {
                showToast("Girdiğiniz mevcut e-posta adresi yanlış.", "error");
                return;
            }

            if (currentEmailInput === newEmailInput) {
                showToast("Yeni e-posta adresi eskisiyle aynı olamaz.", "error");
                return;
            }

            const originalText = btn.textContent;
            btn.textContent = "Güncelleniyor...";
            btn.disabled = true;

            try {
                // Backend'deki UserUpdateDto sadece 'Email' bekliyor
                const response = await fetch(`${API_BASE_URL}/Auth/update-profile`, {
                    method: "PUT",
                    headers: getAuthHeaders(),
                    body: JSON.stringify({ email: newEmailInput })
                });

                const result = await response.json();

                if (response.ok) {
                    showToast("E-posta adresiniz güncellendi! 📧", "success");

                    // LocalStorage'ı güncelle
                    storedState.user.email = newEmailInput;
                    localStorage.setItem("habitQuestState", JSON.stringify(storedState));

                    emailForm.reset();
                    // Yeni e-postayı tekrar inputa yaz
                    document.getElementById("current-email-input").value = newEmailInput;
                } else {
                    showToast(result.message || "Güncelleme başarısız.", "error");
                }
            } catch (error) {
                console.error(error);
                showToast("Sunucu hatası.", "error");
            } finally {
                btn.textContent = originalText;
                btn.disabled = false;
            }
        });
    }
}