const btnR = document.querySelector('.btn-right');
const btnL = document.querySelector('.btn-left');
const tracks = document.querySelector('.tt-row');
const tracksW = tracks.scrollWidth;

btnR.addEventListener('click', _ => {
  tracks.scrollBy({
    left: 200,
    behavior: 'smooth'
  });
});

btnL.addEventListener('click', _ => {
  tracks.scrollBy({
    left: -200,
    behavior: 'smooth'
  });
});