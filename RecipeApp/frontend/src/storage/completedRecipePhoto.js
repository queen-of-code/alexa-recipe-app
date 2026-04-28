import { ref, uploadBytes, getDownloadURL, deleteObject } from 'firebase/storage'
import { storage } from '../firebase'

const MaxBytes = 5 * 1024 * 1024
const AllowedTypes = new Set(['image/jpeg', 'image/png', 'image/webp'])

function extForMime(mime) {
  if (mime === 'image/jpeg') return '.jpg'
  if (mime === 'image/png') return '.png'
  if (mime === 'image/webp') return '.webp'
  return ''
}

/**
 * @param {string} userId
 * @param {string} recipeId
 * @param {File} file
 * @returns {Promise<string>} download URL stored on the recipe document
 */
export async function uploadCompletedRecipePhoto(userId, recipeId, file) {
  if (!file || file.size === 0) throw new Error('Choose an image file.')
  if (file.size > MaxBytes) throw new Error('Image must be 5 MB or smaller.')
  if (!AllowedTypes.has(file.type)) throw new Error('Use JPEG, PNG, or WebP only.')

  const ext = extForMime(file.type)
  if (!ext) throw new Error('Unsupported image type.')

  const objectPath = `users/${userId}/recipes/${recipeId}/completed${ext}`
  const storageRef = ref(storage, objectPath)
  await uploadBytes(storageRef, file, { contentType: file.type })
  return getDownloadURL(storageRef)
}

/**
 * @param {string} downloadUrl from getDownloadURL
 */
export async function deleteCompletedRecipePhotoByUrl(downloadUrl) {
  if (!downloadUrl) return
  const storageRef = ref(storage, downloadUrl)
  await deleteObject(storageRef)
}
