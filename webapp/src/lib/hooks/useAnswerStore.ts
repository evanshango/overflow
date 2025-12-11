import {Answer} from '@/lib/types';
import {create} from 'zustand/react';

type AnswerStore = {
    answer: Answer | null;
    setAnswer: (answer: Answer) => void;
    clearAnswer: () => void;
}

export const useAnswerStore = create<AnswerStore>(
    (set) => ({
        answer: null,
        setAnswer: (answer) => set({answer}),
        clearAnswer: () => set({answer: null})
    })
)