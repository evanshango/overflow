import {getQuestionById} from '@/lib/actions/question-actions';
import {notFound} from 'next/navigation';
import QuestionDetailHeader from '@/app/questions/[id]/QuestionDetailHeader';
import QuestionContent from '@/app/questions/[id]/QuestionContent';
import AnswerContent from '@/app/questions/[id]/AnswerContent';
import AnswerHeader from '@/app/questions/[id]/AnswerHeader';

type Params = Promise<{ id: string }>
export default async function QuestionDetailPage({params}: { params: Params }) {
    const {id} = await params
    const {data: question, error} = await getQuestionById(id)

    if (!question) return notFound()

    return (
        <div className='w-full'>
            <QuestionDetailHeader question={question}/>
            <QuestionContent question={question}/>
            {question.answers.length > 0 && (
                <AnswerHeader answerCount={question.answers.length}/>
            )}
            {question.answers.map(answer => (
                <AnswerContent key={answer.id} answer={answer}/>
            ))}
        </div>
    );
}