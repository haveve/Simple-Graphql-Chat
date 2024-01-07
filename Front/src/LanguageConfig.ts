import i18next from 'i18next';
import { initReactI18next } from 'react-i18next';
import i18nextHttpBackend from 'i18next-http-backend';
import { setCookie, getCookie } from './Features/Functions';


export default function Initiate() {
    i18next
        .use(initReactI18next)
        .use(i18nextHttpBackend)
        .init({
            initImmediate: false,
            lng: "en",
            fallbackLng: "en",
            backend: {
                loadPath: '/languages/{{lng}}.json'
            }
        });
    LoadLanguageFromCookie();
}

export function SetLanguage(language: Languages) {
    setCookie({ expires_second: Number.MAX_VALUE, name: 'language', value: language })
    i18next.changeLanguage(language);
}

export function LoadLanguageFromCookie() {
    i18next.changeLanguage(getCookie('language') ?? Languages.en);
}

export enum Languages {
    en = "en",
    uk = "uk",
    fr = "fr",
    gr = "gr",
    ita = "ita",
    tur = "tur",
    spa = "spa"
}

export const FullLangugeNames = {
    "en": "English",
    "uk": "Ukrainian",
    "fr": "French",
    "gr": "German",
    "ita": "Italian",
    "tur": "Turkish",
    "spa": "Spanish"
}
