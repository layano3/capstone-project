import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}"
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: "#f0f5ff",
          100: "#dce7ff",
          200: "#b9d1ff",
          300: "#87b2ff",
          400: "#568dff",
          500: "#2f65fb",
          600: "#1f49d7",
          700: "#1736b1",
          800: "#0f298b",
          900: "#0a1c61"
        }
      },
      boxShadow: {
        soft: "0 10px 40px -20px rgba(15, 41, 139, 0.45)"
      }
    }
  },
  plugins: []
};

export default config;
