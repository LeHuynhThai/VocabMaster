module.exports = {
  content: ["./src/**/*.{js,jsx,ts,tsx}", "./public/index.html"],
  theme: {
    extend: {
      colors: {
        brand: {
          primary: '#6a11cb',
          secondary: '#2575fc',
          accent: '#4CC9F0',
          warm: '#FF9F1C',
          magenta: '#7209B7',
        },
      },
      backgroundImage: {
        'brand-gradient': 'linear-gradient(135deg, #6a11cb 0%, #2575fc 100%)',
        'brand-gradient-text': 'linear-gradient(45deg, #4CC9F0, #4361EE, #7209B7)',
        'brand-gradient-soft': 'linear-gradient(135deg, rgba(106, 17, 203, 0.12), rgba(37, 117, 252, 0.12))',
        'progress-gradient': 'linear-gradient(90deg, #6a11cb 0%, #2575fc 100%)',
        'success-gradient': 'linear-gradient(90deg, #11998e 0%, #38ef7d 100%)',
      },
      boxShadow: {
        soft: '0 4px 20px rgba(0, 0, 0, 0.08)',
        float: '0 10px 20px rgba(0, 0, 0, 0.12)',
      },
      keyframes: {
        float: {
          '0%, 100%': { transform: 'translateY(0px)' },
          '50%': { transform: 'translateY(-30px)' },
        },
      },
      animation: {
        float: 'float 15s ease-in-out infinite',
        'float-reverse': 'float 18s ease-in-out infinite reverse',
        'float-delayed': 'float 12s ease-in-out infinite 2s',
      },
    },
  },
  plugins: [],
};
