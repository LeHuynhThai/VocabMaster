import React from 'react';
import { Meaning, Pronunciation, Vocabulary } from '../../../types';

const getPartOfSpeechClass = (partOfSpeech: string): string => {
  const pos = partOfSpeech.toLowerCase();

  if (pos.includes('noun')) return 'bg-sky-100 text-sky-900';
  if (pos.includes('verb')) return 'bg-emerald-100 text-emerald-900';
  if (pos.includes('adjective')) return 'bg-amber-100 text-amber-900';
  if (pos.includes('adverb')) return 'bg-fuchsia-100 text-fuchsia-900';
  if (pos.includes('pronoun')) return 'bg-cyan-100 text-cyan-900';
  if (pos.includes('preposition')) return 'bg-teal-100 text-teal-900';
  if (pos.includes('conjunction')) return 'bg-yellow-100 text-yellow-900';
  if (pos.includes('interjection')) return 'bg-pink-100 text-pink-900';

  return 'bg-slate-100 text-slate-800';
};

interface VocabularyDetailCardProps {
  vocabulary: Vocabulary;
  onPlayAudio: (audioUrl: string) => void;
  actions?: React.ReactNode;
}

const VocabularyDetailCard: React.FC<VocabularyDetailCardProps> = ({
  vocabulary,
  onPlayAudio,
  actions,
}) => {
  const translationMissing = !vocabulary.vietnamese;

  return (
    <section className="mt-6 rounded-[1.75rem] bg-white p-6 shadow-[0_2px_10px_rgba(0,0,0,0.1)] sm:p-8">
      <header className="mb-6 border-b border-slate-200 pb-5">
        <h2 className="text-center text-4xl font-bold text-slate-800 sm:text-5xl">
          {vocabulary.word}
        </h2>

        <div
          className={[
            'mx-auto mt-4 max-w-3xl rounded-2xl border-2 p-4 text-center',
            translationMissing
              ? 'border-rose-500 bg-rose-50'
              : 'border-emerald-500 bg-emerald-50',
          ].join(' ')}
        >
          <h3
            className={[
              'mb-2 text-lg font-semibold',
              translationMissing ? 'text-rose-600' : 'text-emerald-700',
            ].join(' ')}
          >
            Nghĩa tiếng Việt:
          </h3>
          <p
            className={[
              'm-0 text-2xl font-semibold',
              translationMissing ? 'italic text-rose-600' : 'text-slate-800',
            ].join(' ')}
          >
            {vocabulary.vietnamese || 'Chưa có bản dịch'}
          </p>
        </div>

        {vocabulary.pronunciations && vocabulary.pronunciations.length > 0 && (
          <div className="mt-4 flex flex-wrap justify-center gap-4">
            {vocabulary.pronunciations.map((pronunciation: Pronunciation, index: number) => (
              <div key={index} className="flex items-center gap-2 rounded-full bg-slate-50 px-3 py-2">
                {pronunciation.text && (
                  <span className="text-sm text-slate-600">{pronunciation.text}</span>
                )}
                {pronunciation.audio && (
                  <button
                    type="button"
                    className="border-0 bg-transparent p-0 text-xl text-sky-600 transition hover:text-sky-700"
                    onClick={() => onPlayAudio(pronunciation.audio)}
                    aria-label="Phát âm"
                  >
                    <i className="bi bi-play-circle-fill"></i>
                  </button>
                )}
              </div>
            ))}
          </div>
        )}
      </header>

      {vocabulary.meanings && vocabulary.meanings.length > 0 && (
        <div className="space-y-6">
          {vocabulary.meanings.map((meaning: Meaning, index: number) => (
            <section key={index} className="rounded-2xl bg-slate-50 p-5">
              <div className="mb-3">
                <span
                  className={[
                    'inline-flex rounded-full px-3 py-1 text-sm font-semibold',
                    getPartOfSpeechClass(meaning.partOfSpeech),
                  ].join(' ')}
                >
                  {meaning.partOfSpeech}
                </span>
              </div>

              <div className="space-y-4">
                {meaning.definitions.map((definition, defIndex) => (
                  <div key={defIndex} className="border-l-2 border-slate-200 pl-4">
                    <p className="m-0 text-lg text-slate-800">
                      <span className="mr-2 font-semibold text-slate-500">{defIndex + 1}.</span>
                      {definition.text}
                    </p>

                    {definition.example && (
                      <p className="mt-2 pl-4 text-sm italic text-slate-500">
                        <i className="bi bi-quote mr-1"></i>
                        {definition.example}
                      </p>
                    )}

                    {definition.synonyms && definition.synonyms.length > 0 && (
                      <div className="mt-2 text-sm">
                        <span className="mr-2 font-semibold text-slate-600">Từ đồng nghĩa:</span>
                        <span className="text-sky-900">{definition.synonyms.join(', ')}</span>
                      </div>
                    )}

                    {definition.antonyms && definition.antonyms.length > 0 && (
                      <div className="mt-2 text-sm">
                        <span className="mr-2 font-semibold text-slate-600">Từ trái nghĩa:</span>
                        <span className="text-rose-800">{definition.antonyms.join(', ')}</span>
                      </div>
                    )}
                  </div>
                ))}
              </div>
            </section>
          ))}
        </div>
      )}

      {actions && (
        <div className="mt-8 flex flex-wrap justify-center gap-3 border-t border-slate-200 pt-4">
          {actions}
        </div>
      )}
    </section>
  );
};

export default VocabularyDetailCard;