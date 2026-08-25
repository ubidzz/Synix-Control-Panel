const menuButton = document.querySelector('.menu-toggle');
const navigation = document.querySelector('.site-nav');

menuButton?.addEventListener('click', () => {
  const isOpen = navigation.classList.toggle('open');
  menuButton.setAttribute('aria-expanded', String(isOpen));
});

navigation?.addEventListener('click', (event) => {
  if (event.target instanceof HTMLAnchorElement) {
    navigation.classList.remove('open');
    menuButton?.setAttribute('aria-expanded', 'false');
  }
});

const lightbox = document.querySelector('#lightbox');
const lightboxImage = lightbox?.querySelector('img');
const lightboxCaption = lightbox?.querySelector('p');

document.querySelectorAll('.gallery-card, .plain-image-button').forEach((card) => {
  card.addEventListener('click', () => {
    if (!(lightbox instanceof HTMLDialogElement) || !(lightboxImage instanceof HTMLImageElement)) return;
    lightboxImage.src = card.dataset.image ?? '';
    lightboxImage.alt = card.dataset.caption ?? 'Synix screenshot';
    if (lightboxCaption) lightboxCaption.textContent = card.dataset.caption ?? '';
    lightbox.showModal();
  });
});

lightbox?.querySelector('.lightbox-close')?.addEventListener('click', () => lightbox.close());
lightbox?.addEventListener('click', (event) => {
  if (event.target === lightbox) lightbox.close();
});

document.querySelectorAll('[data-copy]').forEach((button) => {
  button.addEventListener('click', async () => {
    const value = button.dataset.copy ?? '';
    try {
      await navigator.clipboard.writeText(value);
      const previous = button.textContent;
      button.textContent = 'Copied';
      setTimeout(() => { button.textContent = previous; }, 1500);
    } catch {
      button.textContent = 'Select command';
    }
  });
});

const year = document.querySelector('#year');
if (year) year.textContent = String(new Date().getFullYear());

const gameSearch = document.querySelector('[data-game-search]');
gameSearch?.addEventListener('input', () => {
  const query = gameSearch.value.trim().toLowerCase();
  document.querySelectorAll('[data-game]').forEach((card) => {
    card.hidden = query.length > 0 && !card.dataset.game.includes(query);
  });
});

const helpSearch = document.querySelector('[data-help-search]');
const helpCount = document.querySelector('[data-help-count]');
helpSearch?.addEventListener('input', () => {
  const query = helpSearch.value.trim().toLowerCase();
  let matches = 0;
  document.querySelectorAll('[data-help-article]').forEach((article) => {
    const visible = query.length === 0 || article.textContent.toLowerCase().includes(query);
    article.hidden = !visible;
    if (visible) matches += 1;
  });
  document.querySelectorAll('[data-help-category]').forEach((category) => {
    category.hidden = !category.querySelector('[data-help-article]:not([hidden])');
  });
  if (helpCount) helpCount.textContent = `${matches} help article${matches === 1 ? '' : 's'}`;
});
