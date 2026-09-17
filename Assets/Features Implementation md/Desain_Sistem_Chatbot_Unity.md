# Desain Sistem Hybrid Chatbot untuk Game Unity

Dokumen ini merangkum arsitektur dan kebutuhan sistem untuk mengimplementasikan chatbot pemandu di dalam game Unity, menggunakan pendekatan hibrida (Offline Rule-Based + Online LLM API).

---

## 1. Arsitektur Sistem (Data Flow)

Sistem dirancang modular untuk memastikan kecepatan respons dan efisiensi biaya. Berikut adalah alur pemrosesan pesan:

1.  **Input Pemain:** Pemain memasukkan pertanyaan melalui antarmuka pengguna (UI) di Unity.
2.  **Pemrosesan Awal (Router):** Teks dari pemain diterima oleh komponen `ChatRouter`. Teks akan distandarisasi (misalnya diubah menjadi huruf kecil) untuk memudahkan pencocokan.
3.  **Pencarian Kata Kunci (Offline Layer):**
    *   Router membandingkan input dengan daftar kata kunci di `OfflineDatabase`.
    *   **Jika Cocok:** Router langsung mengambil jawaban statis yang sudah disiapkan dan mengirimkannya ke UI. Waktu respons instan, tanpa biaya API, tanpa butuh internet.
4.  **Fallback ke API (Online Layer):**
    *   **Jika Tidak Cocok:** Jika pertanyaan terlalu kompleks dan tidak ada kata kunci yang cocok, Router meneruskan teks ke `APIClient`.
    *   `APIClient` membungkus teks bersama konteks game (System Prompt) lalu mengirimkannya ke server LLM (misal: OpenAI) via `UnityWebRequest`.
    *   Sistem menunggu balasan JSON, mengekstrak jawaban, dan mengirimkannya ke UI.
5.  **Output:** UI merender balasan bot ke dalam tampilan obrolan.

---

## 2. Kebutuhan UI (Unity Canvas)

Untuk antarmuka yang interaktif, elemen-elemen UI berikut diperlukan:

*   **Scroll View:** Komponen utama untuk menampilkan riwayat pesan, memungkinkan pemain menggulir pesan lama.
*   **Prefab Chat Bubble:** Objek UI (menggunakan `TextMeshProUGUI`) yang akan di-*instantiate* setiap kali ada pesan masuk/keluar. Idealnya ada dua versi: satu untuk pemain (misal rata kanan, warna biru) dan satu untuk bot (rata kiri, warna abu-abu).
*   **TMP_InputField:** Kotak tempat pemain mengetik pesan.
*   **Send Button:** Tombol UI untuk memicu fungsi pengiriman pesan.

---

## 3. Struktur Script C#

Sistem dipecah menjadi beberapa *script* agar mudah dikelola dan dikembangkan:

### `ChatUIManager.cs`
**Fokus:** Visual dan Presentasi.
*   Menangkap input dari tombol "Kirim" atau tombol `Enter`.
*   Melakukan *instantiate* prefab Chat Bubble ke dalam *Content* dari Scroll View.
*   Memastikan Scroll View selalu menggulir ke bawah saat ada pesan baru.

### `ChatRouter.cs`
**Fokus:** Logika Penentu Keputusan (The "Brain").
*   Menerima pesan dari `ChatUIManager`.
*   Mengeksekusi logika `if/else` sederhana: apakah pesan masuk kategori Offline (berisi kata kunci tertentu) atau kategori Online (perlu dikirim ke API).
*   Berkomunikasi dengan `OfflineDatabase` dan `APIClient`.

### `OfflineDatabase.cs`
**Fokus:** Penyimpanan Data Lokal.
*   Berisi `Dictionary<string, string>` atau sistem pembacaan file JSON lokal.
*   Menyimpan pasangan kata kunci dan jawabannya. (Contoh: `{"crafting", "Untuk membuat senjata, pergi ke tungku pandai besi."}`).

### `APIClient.cs`
**Fokus:** Komunikasi Jaringan (Networking).
*   Menyimpan URL Endpoint API dan *API Key* (sebaiknya dienkripsi atau disimpan aman).
*   Menggunakan `UnityEngine.Networking.UnityWebRequest` untuk mengirim method POST.
*   Menyisipkan "System Prompt" (instruksi perilaku bot) sebelum pertanyaan pemain.
*   Mengurai (*parsing*) respons JSON dari server kembali menjadi teks biasa.

---

## 4. Kebutuhan Eksternal & Dependencies

*   **API Key LLM:** Akun aktif di penyedia layanan AI (seperti OpenAI, Anthropic, atau Eden AI) untuk mendapatkan *Secret Key*.
*   **Newtonsoft.Json:** *Package* Unity yang esensial untuk melakukan serialisasi dan deserialisasi data JSON saat berkomunikasi dengan API. (Dapat diinstal via Unity Package Manager -> Add package from git URL -> `com.unity.nuget.newtonsoft-json`).
