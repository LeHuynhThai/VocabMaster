/**
 * Application constants
 */

// API endpoints
export const API_ENDPOINTS = {
  LOGIN: '/api/account/login',
  REGISTER: '/api/account/register',
  LOGOUT: '/api/account/logout',
  CURRENT_USER: '/api/account/currentuser',
  RANDOM_WORD: '/api/wordgenerator/getrandomword',
  LOOKUP_WORD: (word: string) => `/api/wordgenerator/lookup/${word}`,
  LEARNED_WORDS: '/api/learnedword',
  DELETE_LEARNED_WORD: (id: number) => `/api/learnedword/${id}`
};

// Routes
export const ROUTES = {
  HOME: '/',
  LOGIN: '/login',
  REGISTER: '/register',
  WORD_GENERATOR: '/wordgenerator',
  LEARNED_WORDS: '/learnedwords'
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
  ERROR_REMOVE_WORD: 'Không thể xóa từ vựng. Vui lòng thử lại sau.'
}; 