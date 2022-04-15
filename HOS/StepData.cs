using static Client.Tui;

namespace Client
{
    public class StepData
    {
        public Step Step { get; private set; }
        public int Id { get; private set; }

        public StepData(Step step, int id)
        {
            (Step, Id) = (step, id);
        }

        public StepData(Step step)
        {
            new StepData(step, 0);
        }

        public void Deconstruct(out Step step, out int id)
        {
            step = Step;
            id = Id;
        }

    }
}
