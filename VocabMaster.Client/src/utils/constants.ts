/**
 * Application routes
 */
export const ROUTES = {
  HOME: '/home',
  LOGIN: '/login',
  REGISTER: '/register',
  WORD_GENERATOR: '/word-generator',
  LEARNED_WORDS: '/learned-words',
  TRANSLATION: '/translation',
  NOT_FOUND: '/not-found'
};

/**
 * API endpoints
 */
export const API_ENDPOINTS = {
  LOGIN: '/api/account/login',
  REGISTER: '/api/account/register',
  LOGOUT: '/api/account/logout',
  CURRENT_USER: '/api/account/currentuser',
  RANDOM_WORD: '/api/wordgenerator/getrandomword',
  NEW_RANDOM_WORD: '/api/wordgenerator/getnewrandomword',
  LOOKUP_WORD: '/api/wordgenerator/lookup',
  IS_LEARNED: '/api/wordgenerator/islearned',
  LEARNED_WORDS: '/api/learnedword',
  TRANSLATE_EN_VI: '/api/translation/en-to-vi',
  TRANSLATE: '/api/translation/translate'
};

/**
 * Local storage keys
 */
export const STORAGE_KEYS = {
  AUTH_TOKEN: 'auth_token',
  USER: 'user_info'
};

// Messages
export const MESSAGES = {
  LOGIN_SUCCESS: 'Đăng nhập thành công!',
  REGISTER_SUCCESS: 'Đăng ký thành công! Vui lòng đăng nhập.',
  LOGOUT_SUCCESS: 'Đăng xuất thành công!',
  WORD_SAVED: 'Từ vựng đã được lưu thành công!',
  WORD_REMOVED: 'Từ vựng đã được xóa thành công!',
  LOGIN_REQUIRED: 'Vui lòng đăng nhập để tiếp tục',
  ERROR_GENERIC: 'Đã xảy ra lỗi. Vui lòng thử lại sau.',
  ERROR_LOGIN: 'Đăng nhập thất bại. Vui lòng kiểm tra thông tin đăng nhập.',
  ERROR_REGISTER: 'Đăng ký thất bại. Vui lòng thử lại.',
  ERROR_FETCH_WORDS: 'Không thể tải danh sách từ vựng. Vui lòng thử lại sau.',
  ERROR_SAVE_WORD: 'Không thể lưu từ vựng. Vui lòng thử lại sau.',
  ERROR_REMOVE_WORD: 'Không thể xóa từ vựng. Vui lòng thử lại sau.',
  ERROR_TRANSLATION: 'Không thể dịch văn bản. Vui lòng thử lại sau.'
}; 