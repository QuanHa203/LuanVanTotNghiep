const imageContainerElement = document.getElementById("image-container");
const videoContainerElement = document.getElementById("video-container");

const btnImageNav = document.getElementById("btn-image-nav");
const btnVideoNav = document.getElementById("btn-video-nav");

const overlayMediaSectionElement = document.getElementById("overlay-media-section");
const overlayMediaTitle = document.getElementById("overlay-media-title");
const overlayMediaVideoControls = document.getElementById("overlay-media-video-controls");

const mediaImageSource = document.getElementById("media-image-src");
const mediaVideoSource = document.getElementById("media-video-src");

const btnCloseOverlayMedia = document.getElementById("overlay-media-close-btn");

const playBtn = document.querySelector('.fa-play').parentElement;
const progressBar = document.querySelector('.progress-bar');
const timeDisplay = document.querySelector('.time-display');
const volumeSlider = document.querySelector('.volume-slider');

const btnDownload = overlayMediaSectionElement.querySelector(".btn-download");
const btnDelete = overlayMediaSectionElement.querySelector(".btn-delete");

let mediaPath;
let mediaBoxChild;

if (mediaVideoSource) {
    playBtn.addEventListener('click', function () {
        if (mediaVideoSource.paused) {
            mediaVideoSource.play();
            playBtn.innerHTML = '<i class="fas fa-pause"></i>';
        }
        else {
            mediaVideoSource.pause();
            playBtn.innerHTML = '<i class="fas fa-play"></i>';
        }
    });

    mediaVideoSource.addEventListener('timeupdate', (e) => {
        if (!mediaVideoSource.duration) {
            progressBar.style.width = '0%';
            timeDisplay.textContent = '00:00 / 00:00';
            return;
        }
        const progress = (mediaVideoSource.currentTime / mediaVideoSource.duration) * 100;
        progressBar.style.width = progress + '%';

        const currentMinutes = Math.floor(mediaVideoSource.currentTime / 60);
        const currentSeconds = Math.floor(mediaVideoSource.currentTime % 60);
        const durationMinutes = Math.floor(mediaVideoSource.duration / 60);
        const durationSeconds = Math.floor(mediaVideoSource.duration % 60);

        timeDisplay.textContent =
            `${currentMinutes.toString().padStart(2, '0')}:${currentSeconds.toString().padStart(2, '0')} /
            ${durationMinutes.toString().padStart(2, '0')}:${durationSeconds.toString().padStart(2, '0')}`;
    });

    document.querySelector('.progress-container').addEventListener('click', (e) => {
        const percent = e.offsetX / e.target.offsetWidth;
        mediaVideoSource.currentTime = percent * mediaVideoSource.duration;
    });

    volumeSlider.addEventListener('input', (e) => {
        mediaVideoSource.volume = this.value;
    });
}


btnImageNav.addEventListener("click", (e) => {
    imageContainerElement.style.display = "flex";
    videoContainerElement.style.display = "none";

    setNavBtnActive(btnImageNav);
});

btnVideoNav.addEventListener("click", (e) => {
    imageContainerElement.style.display = "none";
    videoContainerElement.style.display = "flex";

    setNavBtnActive(btnVideoNav);
});

btnCloseOverlayMedia.addEventListener("click", (e) => {
    overlayMediaSectionElement.style.display = "none";

})

btnDelete.addEventListener("click", deleteMedia);

function showOverlayMediaSectionElement(element) {
    let src = element.src;
    let srcSplit = src.split("/");
    let mediaFileName = srcSplit[srcSplit.length - 1];

    overlayMediaTitle.innerText = mediaFileName;

    btnDownload.href = src;
    mediaPath = src.split("Medias/")[1]
    mediaBoxChild = element.closest(".media-box-child");

    if (element.tagName === 'IMG') {
        mediaImageSource.src = src;

        mediaImageSource.style.display = "block";

        mediaVideoSource.style.display = "none";

        overlayMediaVideoControls.style.display = "none";
    }

    else if (element.tagName === 'VIDEO') {
        mediaVideoSource.src = src;

        mediaVideoSource.style.display = "block";
        mediaImageSource.style.display = "none";

        overlayMediaVideoControls.style.display = "flex";
        mediaVideoSource.play();
        playBtn.innerHTML = '<i class="fas fa-pause"></i>';
    }

    overlayMediaSectionElement.style.display = "flex";
}

function setNavBtnActive(clickedBtn) {
    const buttons = document.querySelectorAll('.nav-btn');

    buttons.forEach(btn => btn.classList.remove('nav-btn-active'));

    clickedBtn.classList.add('nav-btn-active');
}

function deleteMedia() {
    if (confirm(`Bạn có muốn xóa file này không?`)) {
        fetch(`${document.location.origin}/DataStatistics/DeleteMedia`, {
            method: "POST",
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(mediaPath)
        })
            .then(response => {
                if (response.ok) {
                    alert("Xóa thành công!");
                    mediaBoxChild.remove();
                }
                else if (response.status == 403)
                    alert("Bạn không có quyền xóa file này!");
                else
                    alert("Xóa thất bại")

                overlayMediaSectionElement.style.display = "none";
            })
    }
}