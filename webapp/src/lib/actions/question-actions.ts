'use server'

import {Answer, Question} from '@/lib/types';
import {fetchClient} from '@/lib/fetchClient';
import {QuestionSchema} from '@/lib/schemas/questionSchema';
import {AnswerSchema} from '@/lib/schemas/answerSchema';
import {revalidatePath} from 'next/cache';

export async function getQuestions(tag?: string) {
    let url = '/api/v1/questions';
    if (tag) url += '?tag=' + tag;
    return fetchClient<Question[]>(url, 'GET')
}

export async function getQuestionById(id: string) {
    return fetchClient<Question>(`/api/v1/questions/${id}`, 'GET');
}

export async function searchQuestions(query: string) {
    return fetchClient<Question[]>(`/api/v1/search?query=${query}`, 'GET')
}

export async function postQuestion(question: QuestionSchema) {
    return fetchClient<Question>('/api/v1/questions', 'POST', {body: question});
}

export async function updateQuestion(question: QuestionSchema, id: string) {
    return fetchClient(`/api/v1/questions/${id}`, 'PUT', {body: question});
}

export async function deleteQuestion(id: string) {
    return fetchClient(`/api/v1/questions/${id}`, 'DELETE')
}

export async function postAnswer(data: AnswerSchema, questionId: string) {
    const result = await fetchClient<Answer>(
        `/api/v1/questions/${questionId}/answers`, 'POST', {body: data}
    );

    revalidatePath(`/questions/${questionId}`);

    return result;
}

export async function editAnswer(answerId: string, questionId: string, content: AnswerSchema) {
    const result = await fetchClient(
        `/api/v1/questions/${questionId}/answers/${answerId}`, 'PUT', {body: content}
    );

    revalidatePath(`/questions/${questionId}`);

    return result;
}

export async function deleteAnswer(answerId: string, questionId: string) {
    const result = await fetchClient(
        `/api/v1/questions/${questionId}/answers/${answerId}`, 'DELETE'
    )

    revalidatePath(`/questions/${questionId}`);

    return result;
}