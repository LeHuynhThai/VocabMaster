// Lightweight logger that masks sensitive fields and only logs in development
const isDev = process.env.NODE_ENV === 'development';

function maskSensitive(obj: any): any {
  if (!obj || typeof obj !== 'object') return obj;
  try {
    const cloned = JSON.parse(JSON.stringify(obj));
    const sensitiveKeywords = ['token', 'accesstoken', 'authorization', 'password', 'refresh', 'secret', 'user', 'userid', 'username'];
    const walk = (o: any) => {
      if (!o || typeof o !== 'object') return;
      if (Array.isArray(o)) return o.forEach(walk);
      for (const k of Object.keys(o)) {
        const lower = k.toLowerCase();
        if (sensitiveKeywords.some(sk => lower.includes(sk))) {
          o[k] = '***';
        } else if (typeof o[k] === 'object') {
          walk(o[k]);
        }
      }
    };
    walk(cloned);
    return cloned;
  } catch (e) {
    return obj;
  }
}

export const logger = {
  debug: (...args: any[]) => {
    if (!isDev) return;
    const sanitized = args.map(a => (typeof a === 'object' ? maskSensitive(a) : a));
    // eslint-disable-next-line no-console
    console.debug(...sanitized);
  },
  info: (...args: any[]) => {
    if (!isDev) return;
    // eslint-disable-next-line no-console
    console.info(...args);
  },
  warn: (...args: any[]) => {
    if (!isDev) return;
    // eslint-disable-next-line no-console
    console.warn(...args);
  },
  error: (...args: any[]) => {
    if (!isDev) return;
    const sanitized = args.map(a => (typeof a === 'object' ? maskSensitive(a) : a));
    // eslint-disable-next-line no-console
    console.error(...sanitized);
  }
};

export default logger;
