import { LEVELS } from './config.js';
import { parseJwt } from './utils.js';

export function loadUserData() {
    let state = JSON.parse(localStorage.getItem("habitQuestState")) || { user: { points: 0 } };
    const points = state.user.points || 0;
    const token = localStorage.getItem("jwtToken");
    let username = "Arkadaş";
    if (token) {
        const decodedToken = parseJwt(token);
        if (decodedToken) {
            username = decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] 
                || decodedToken["unique_name"] || decodedToken["Name"] || decodedToken["name"] 
                || decodedToken["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] || "Arkadaş";
        }
    }
    const userDisplay = document.getElementById("user-display");
    if (userDisplay) userDisplay.textContent = `Merhaba, ${username}!`;
    const pointsDisplay = document.getElementById("user-points");
    if (pointsDisplay) pointsDisplay.textContent = points;

    const currentLevelIndex = Math.min(Math.floor(points / 10), LEVELS.length - 1);
    const currentLevelData = LEVELS[currentLevelIndex];
    const nextLevelData = LEVELS[currentLevelIndex + 1];
    let progressPercent = 100;
    const progressText = document.getElementById("progress-text");
    if (nextLevelData) {
        const pointsInLevel = points % 10;
        progressPercent = (pointsInLevel / 10) * 100;
        if (progressText) progressText.textContent = `İlerleme Durumu: %${Math.round(progressPercent)}`;
    } else { if (progressText) progressText.textContent = "Maksimum Seviye!"; }
    const levelName = document.getElementById("user-level-name");
    if (levelName) levelName.textContent = `${currentLevelData.name} (Lv.${currentLevelData.level})`;
    const progressBar = document.getElementById("level-progress-bar");
    if (progressBar) progressBar.style.width = `${progressPercent}%`;
}
export function updateLocalPoints(amount) {
    let state = JSON.parse(localStorage.getItem("habitQuestState")) || { user: { points: 0 } };
    state.user.points = (state.user.points || 0) + amount;
    if (state.user.points < 0) state.user.points = 0;
    localStorage.setItem("habitQuestState", JSON.stringify(state));
    loadUserData();
}