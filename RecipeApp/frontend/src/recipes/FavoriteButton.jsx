export default function FavoriteButton({ isFavorite, onToggle }) {
  const label = isFavorite ? 'Remove from favorites' : 'Add to favorites'
  return (
    <button
      type="button"
      aria-pressed={Boolean(isFavorite)}
      aria-label={label}
      onClick={onToggle}
      className={`text-lg leading-none px-1 ${isFavorite ? 'text-amber-500' : 'text-gray-400 hover:text-amber-500'}`}
    >
      <span aria-hidden="true">{isFavorite ? '★' : '☆'}</span>
    </button>
  )
}
