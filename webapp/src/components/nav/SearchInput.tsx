'use client'

import {MagnifyingGlassIcon} from '@heroicons/react/24/solid';
import {Input} from '@heroui/input';
import {ChangeEvent, useEffect, useRef, useState} from 'react';
import {Question} from '@/lib/types';
import {searchQuestions} from '@/lib/actions/question-actions';
import {Listbox, ListboxItem} from '@heroui/listbox';

export default function SearchInput() {
    const [query, setQuery] = useState('');
    const [loading, setLoading] = useState(false);
    const [results, setResults] = useState<Question[] | null>(null);
    const [showDropdown, setShowDropdown] = useState(false);
    const timeoutRef = useRef<NodeJS.Timeout | null>(null);

    const handleQueryChange = (e: ChangeEvent<HTMLInputElement>) => {
        const value = e.target.value;
        setQuery(value);

        if (!value) {
            setResults(null);
            setShowDropdown(false);
        }
    };

    useEffect(() => {
        if (timeoutRef.current) clearTimeout(timeoutRef.current)

        if (!query) return;

        timeoutRef.current = setTimeout(async () => {
            setLoading(true);
            const {data: questions} = await searchQuestions(query);
            setResults(questions);
            setLoading(false);
            setShowDropdown(true);
        }, 300)

        return () => {
            if (timeoutRef.current) {
                clearTimeout(timeoutRef.current);
            }
        };
    }, [query]);

    const onAction = () => {
        setQuery('');
        setResults(null);
    }

    return (
        <div className='flex flex-col w-full'>
            <Input startContent={<MagnifyingGlassIcon className='size-6'/>}
                   className='ml-6'
                   type='search'
                   placeholder='Search'
                   value={query}
                   onChange={handleQueryChange}
            />
            {showDropdown && results && !loading && (
                <div className='absolute top-full z-50 bg-white dark:bg-default-50 
                    shadow-lg border-2 border-default-500 w-[50%]'
                >
                    <Listbox
                        aria-label='search-results'
                        onAction={onAction}
                        items={results}
                        className='flex flex-col overflow-y-auto'
                    >
                        {question => (
                            <ListboxItem
                                textValue={question.title}
                                href={`/questions/${question.id}`}
                                key={question.id}
                                startContent={
                                    <div className='flex flex-col h-14 min-w-14 justify-center 
                                    items-center border border-success rounded-md'
                                    >
                                        <span>{question.answerCount}</span>
                                        <span className='text-xs'>answers</span>
                                    </div>
                                }
                            >
                                <div>
                                    <div className='font-semibold'>
                                        {question.title}
                                    </div>
                                    <div className='text-xs opacity-60 line-clamp-2'>
                                        {question.content}
                                    </div>
                                </div>
                            </ListboxItem>
                        )}
                    </Listbox>
                </div>
            )}
        </div>
    );
}