import {getTags} from '@/lib/actions/tag-actions';
import TagCard from '@/app/tags/TagCard';
import TagHeader from '@/app/tags/TagHeader';

export default async function TagsPage() {
    const {data: tags, error} = await getTags();

    if (error) throw error;

    return (
        <div className='w-full px-6'>
            <TagHeader/>
            <div className='grid grid-cols-3 gap-4'>
                {tags?.map(tag => (
                    <TagCard tag={tag} key={tag.id}/>
                ))}
            </div>
        </div>
    );
}