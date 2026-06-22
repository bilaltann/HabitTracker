FROM nginx:alpine
# Klasörün içindeki tüm HTML/JS/CSS dosyalarını Nginx'e kopyala
COPY ["Habit-Tracker Frontend", "/usr/share/nginx/html"]
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]