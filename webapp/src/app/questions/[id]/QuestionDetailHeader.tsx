'use client'
import {Question} from '@/lib/types';
import {Button} from '@heroui/button';
import Link from 'next/link';
import {fuzzyTimeAgo} from '@/lib/util';
import DeleteQuestionButton from '@/app/questions/[id]/DeleteQuestionButton';

type Props = {
    question: Question,
    askerId?: string,
}
export default function QuestionDetailHeader({question, askerId}: Props) {
    return (
        <div className='flex flex-col w-full border-b gap-4 pb-4 px-6'>
            <div className='flex justify-between gap-4'>
                <div className='text-3xl font-semibold first-letter:uppercase'>
                    {question.title}
                </div>
                <Button as={Link} href='/questions/ask' color='secondary' className='rounded-full w-[20%]'>
                    Ask Question
                </Button>
            </div>
            <div className='flex justify-between items-center'>
                <div className='flex items-center gap-6'>
                    <div className='flex items-center gap-3'>
                        <span className='text-foreground-500'>Asked</span>
                        <span>{fuzzyTimeAgo(question.createdAt)}</span>
                    </div>
                    {question.updatedAt && (
                        <div className='flex items-center gap-3'>
                            <span className='text-foreground-500'>Modified</span>
                            <span>{fuzzyTimeAgo(question.updatedAt)}</span>
                        </div>
                    )}
                    <div className='flex items-center gap-3'>
                        <span className='text-foreground-500'>Viewed</span>
                        <span>{question.viewCount + 1} times</span>
                    </div>
                </div>
                {askerId === question.askerId && (
                    <div className='flex items-center gap-3'>
                        <Button
                            as={Link}
                            href={`/questions/${question.id}/edit`}
                            size='sm'
                            variant='faded'
                            color='primary'
                            className='rounded-full'
                        >
                            Edit
                        </Button>
                        <DeleteQuestionButton questionId={question.id}/>
                    </div>
                )}
            </div>
        </div>
    );
}