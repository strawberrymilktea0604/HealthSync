/**
 * Add cache busting timestamp to image URL to force browser reload
 * @param imageUrl - Original image URL
 * @returns Image URL with timestamp query parameter
 */
export const addCacheBuster = (imageUrl: string | null | undefined): string => {
  if (!imageUrl) return '';
  
  const separator = imageUrl.includes('?') ? '&' : '?';
  return `${imageUrl}${separator}t=${Date.now()}`;
};

/**
 * Get image URL with cache busting or fallback placeholder
 * @param imageUrl - Original image URL
 * @param fallback - Fallback URL if image is not available
 * @returns Image URL with cache busting or fallback
 */
export const getImageUrl = (
  imageUrl: string | null | undefined, 
  fallback: string = 'https://placehold.co/400x300'
): string => {
  return imageUrl ? addCacheBuster(imageUrl) : fallback;
};
